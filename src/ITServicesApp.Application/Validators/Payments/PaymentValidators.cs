using FluentValidation;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.Validators.Payments
{
    public class CreatePaymentDtoValidator : AbstractValidator<CreatePaymentDto>
    {
        public CreatePaymentDtoValidator(IBookingRepository bookings)
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0)
                .MustAsync(async (id, ct) => (await bookings.GetByIdAsync(id, ct)) != null)
                .WithMessage("Booking does not exist.");

            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Currency).MaximumLength(10).When(x => x.Currency != null);

            RuleFor(x => x.Method)
                .IsInEnum().WithMessage("Invalid payment method.");

            RuleFor(x => x.BookingId).MustAsync(async (bookingId, ct) =>
            {
                var b = await bookings.GetByIdAsync(bookingId, ct);
                return b != null && b.Status != BookingStatus.Cancelled;
            }).WithMessage("Cannot create a payment for a cancelled booking.");
        }
    }
}
