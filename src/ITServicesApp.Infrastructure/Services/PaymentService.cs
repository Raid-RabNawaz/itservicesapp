using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.UseCases.Bookings.Commands.CompleteBooking;
using ITServicesApp.Application.Options;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Stripe;

namespace ITServicesApp.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly StripeService _stripe;
        private readonly StripeOptions _opts;
        private readonly ICurrentUserService _current;
        private readonly IMediator _mediator;

        public PaymentService(IUnitOfWork uow, IMapper mapper, StripeService stripe, IOptions<StripeOptions> opts, ICurrentUserService current, IMediator mediator)
        {
            _uow = uow;
            _mapper = mapper;
            _stripe = stripe;
            _opts = opts.Value ?? throw new InvalidOperationException("Stripe options are not configured.");
            _current = current;
            _mediator = mediator;
        }

        public async Task<PaymentDto> CreateCashAsync(CreatePaymentDto dto, CancellationToken ct = default)
        {
            var booking = await _uow.Bookings.GetByIdAsync(dto.BookingId, ct) ?? throw new InvalidOperationException("Booking not found.");
            if (booking.Status == BookingStatus.Cancelled) throw new InvalidOperationException("Cannot pay for cancelled booking.");
            await EnsureCanManageBookingAsync(booking, ct);

            var payment = new Payment
            {
                BookingId = dto.BookingId,
                Method = Domain.Enums.PaymentMethod.Cash,
                Amount = dto.Amount,
                Currency = dto.Currency ?? "USD",
                Status = "Succeeded" // tech confirms cash collection in-app
            };

            await _uow.Payments.AddAsync(payment, ct);
            await _uow.SaveChangesAsync(ct);

            await FinalizeBookingIfNeededAsync(booking, ct);

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto> CreateOnlineAsync(CreatePaymentDto dto, CancellationToken ct = default)
        {
            var booking = await _uow.Bookings.GetByIdAsync(dto.BookingId, ct) ?? throw new InvalidOperationException("Booking not found.");
            if (booking.Status == BookingStatus.Cancelled) throw new InvalidOperationException("Cannot pay for cancelled booking.");
            await EnsureCanManageBookingAsync(booking, ct);

            var payment = new Payment
            {
                BookingId = dto.BookingId,
                Method = Domain.Enums.PaymentMethod.Card,
                Amount = dto.Amount,
                Currency = dto.Currency ?? "USD",
                Status = "Pending"
            };

            await _uow.Payments.AddAsync(payment, ct);
            await _uow.SaveChangesAsync(ct);

            // Create Stripe PaymentIntent (pseudo - create & set ProviderPaymentId)
            var providerPaymentId = await _stripe.CreatePaymentIntentAsync(payment.Amount, payment.Currency!, ct);
            payment.ProviderPaymentId = providerPaymentId;
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task HandleStripeEventAsync(string eventId, string type, string providerPaymentId, string? chargeId, string status, decimal? amount, string? currency, CancellationToken ct)
        {
            var payment = await _stripe.ApplyStripeEventAsync(eventId, type, providerPaymentId, chargeId, status, amount, currency, ct);
            if (payment is null) return;

            if (IsSuccessfulStatus(payment.Status))
            {
                var booking = await _uow.Bookings.GetByIdAsync(payment.BookingId, ct);
                if (booking != null)
                {
                    await FinalizeBookingIfNeededAsync(booking, ct);
                }
            }
        }

        public async Task<PaymentDto?> GetByBookingAsync(int bookingId, CancellationToken ct = default)
        {
            var booking = await _uow.Bookings.GetByIdAsync(bookingId, ct) ?? throw new InvalidOperationException("Booking not found.");
            await EnsureCanManageBookingAsync(booking, ct);

            var payment = booking.Payment ?? (await _uow.Payments.ListAsync(p => p.BookingId == bookingId, ct)).FirstOrDefault();
            return payment is null ? null : _mapper.Map<PaymentDto>(payment);
        }

        public async Task HandleStripeWebhookAsync(string payload, string signature, CancellationToken ct)
        {
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(payload, signature, _opts.WebhookSecret);
            }
            catch
            {
                // invalid signature/payload - ignore
                return;
            }

            // Defaults
            string eventId = stripeEvent.Id ?? Guid.NewGuid().ToString("N");
            string type = stripeEvent.Type ?? "unknown";
            string providerPaymentId = string.Empty;
            string? chargeId = null;
            string status = "unknown";
            decimal? amount = null;
            string? currency = null;

            if (stripeEvent.Data?.Object is PaymentIntent intent)
            {
                providerPaymentId = intent.Id;
                status = intent.Status;

                // Handle both nullable and non-nullable Stripe.NET shapes without .HasValue
                long? cents = null;
                try { cents = intent.AmountReceived; } catch { /* property may not exist or be 0 */ }
                if (cents == null || cents == 0) { try { cents = intent.Amount; } catch { } }

                amount = cents.HasValue ? cents.Value / 100m : (decimal?)null;

                // Currency can be null in some events
                currency = intent.Currency?.ToUpperInvariant();

                // Charge id is optional; PaymentIntent may not expose LatestChargeId depending on version
                try { chargeId = intent.LatestChargeId; } catch { /* okay */ }
            }
            else if (stripeEvent.Data?.Object is Charge charge)
            {
                providerPaymentId = charge.PaymentIntentId ?? charge.Id;
                chargeId = charge.Id;
                status = charge.Status;
                amount = charge.Amount / 100m;
                currency = charge.Currency?.ToUpperInvariant();
            }

            await HandleStripeEventAsync(eventId, type, providerPaymentId, chargeId, status, amount, currency, ct);
        }

        private async Task FinalizeBookingIfNeededAsync(Booking booking, CancellationToken ct)
        {
            if (booking.Status == BookingStatus.Completed) return;

            await _mediator.Send(new CompleteBookingCommand(booking.Id, DateTime.UtcNow), ct);
        }

        private static bool IsSuccessfulStatus(string status) => string.Equals(status, "succeeded", StringComparison.OrdinalIgnoreCase)
                                                                  || string.Equals(status, "paid", StringComparison.OrdinalIgnoreCase);

        private async Task EnsureCanManageBookingAsync(Booking booking, CancellationToken ct)
        {
            var currentUserId = _current.UserIdInt;
            if (currentUserId <= 0)
                throw new UnauthorizedAccessException("Authentication required.");

            var role = GetCurrentRole();
            if (role == UserRole.Admin)
                return;

            if (booking.UserId == currentUserId)
                return;

            if (role == UserRole.Technician)
            {
                var technicians = await _uow.Technicians.ListAsync(t => t.UserId == currentUserId, ct);
                if (technicians.Any(t => t.Id == booking.TechnicianId))
                    return;
            }

            throw new UnauthorizedAccessException("You cannot access payments for this booking.");
        }

        private UserRole GetCurrentRole()
        {
            var raw = _current.Role;
            return Enum.TryParse<UserRole>(raw, true, out var parsed) ? parsed : UserRole.Customer;
        }
    }
}
