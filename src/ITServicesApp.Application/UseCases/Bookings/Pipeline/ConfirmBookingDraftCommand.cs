using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.UseCases.Bookings.Commands.CreateBooking;
using ITServicesApp.Application.UseCases.Bookings.Commands.CreateGuestBooking;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Pipeline
{
    public sealed record ConfirmBookingDraftCommand(Guid DraftId, BookingPipelineConfirmDto Dto) : IRequest<BookingPipelineSubmissionResultDto>;

    public sealed class ConfirmBookingDraftCommandHandler : IRequestHandler<ConfirmBookingDraftCommand, BookingPipelineSubmissionResultDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMediator _mediator;
        private readonly IBookingAssignmentService _assignment;

        public ConfirmBookingDraftCommandHandler(IUnitOfWork uow, IMediator mediator, IBookingAssignmentService assignment)
        {
            _uow = uow;
            _mediator = mediator;
            _assignment = assignment;
        }

        public async Task<BookingPipelineSubmissionResultDto> Handle(ConfirmBookingDraftCommand request, CancellationToken ct)
        {
            var draft = await _uow.BookingDrafts.GetAsync(request.DraftId, ct)
                        ?? throw new InvalidOperationException("Booking draft not found.");

            if (draft.Status != BookingDraftStatus.Pending && draft.Status != BookingDraftStatus.Submitted)
            {
                throw new InvalidOperationException("Draft is no longer active.");
            }

            if (draft.ExpiresAtUtc.HasValue && draft.ExpiresAtUtc.Value <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Booking draft has expired.");
            }

            if (!draft.ServiceIssueId.HasValue || !draft.ServiceCategoryId.HasValue)
            {
                throw new InvalidOperationException("Service selection is incomplete.");
            }

            if (string.IsNullOrWhiteSpace(draft.AddressLine1) || string.IsNullOrWhiteSpace(draft.City) ||
                string.IsNullOrWhiteSpace(draft.PostalCode) || string.IsNullOrWhiteSpace(draft.Country))
            {
                throw new InvalidOperationException("Address information is incomplete.");
            }

            if (!draft.TechnicianId.HasValue || !draft.SlotStartUtc.HasValue || !draft.SlotEndUtc.HasValue)
            {
                throw new InvalidOperationException("A technician and slot must be selected before confirmation.");
            }

            if (!draft.Items.Any())
            {
                throw new InvalidOperationException("At least one service item is required.");
            }

            var slotStartUtc = draft.SlotStartUtc.Value;
            var slotEndUtc = draft.SlotEndUtc.Value;

            if (slotEndUtc <= slotStartUtc)
            {
                throw new InvalidOperationException("Selected slot is invalid.");
            }

            var durationMinutes = draft.EstimatedDurationMinutes > 0
                ? draft.EstimatedDurationMinutes
                : (int)Math.Round((slotEndUtc - slotStartUtc).TotalMinutes);

            if (durationMinutes <= 0)
            {
                durationMinutes = 60;
                slotEndUtc = slotStartUtc.AddMinutes(durationMinutes);
            }
            else
            {
                slotEndUtc = slotStartUtc.AddMinutes(durationMinutes);
            }

            draft.EstimatedDurationMinutes = durationMinutes;
            draft.SlotEndUtc = slotEndUtc;

            var stillAvailable = await _assignment.IsTechnicianAvailableAsync(
                draft.TechnicianId.Value,
                slotStartUtc,
                durationMinutes,
                ct);

            if (!stillAvailable)
            {
                throw new InvalidOperationException("Selected technician is no longer available for the requested time.");
            }

            var confirmDto = request.Dto ?? new BookingPipelineConfirmDto();
            if (confirmDto.PreferredPaymentMethod.HasValue)
            {
                draft.PreferredPaymentMethod = confirmDto.PreferredPaymentMethod.Value;
            }

            if (draft.UserId.HasValue)
            {
                var bookingDto = new CreateBookingDto
                {
                    UserId = draft.UserId.Value,
                    TechnicianId = draft.TechnicianId.Value,
                    ServiceCategoryId = draft.ServiceCategoryId,
                    ServiceIssueId = draft.ServiceIssueId,
                    Start = draft.SlotStartUtc.Value,
                    End = draft.SlotEndUtc.Value,
                    PreferredPaymentMethod = draft.PreferredPaymentMethod,
                    Notes = draft.Notes,
                    Address = new BookingAddressDto
                    {
                        Line1 = draft.AddressLine1!,
                        Line2 = draft.AddressLine2,
                        City = draft.City!,
                        State = draft.State,
                        PostalCode = draft.PostalCode!,
                        Country = draft.Country!
                    },
                    Items = draft.Items.Select(i => new CreateBookingItemDto
                    {
                        ServiceIssueId = i.ServiceIssueId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Notes = i.Notes
                    }).ToList(),
                    ClientRequestId = confirmDto.ClientRequestId
                };

                var booking = await _mediator.Send(new CreateBookingCommand(bookingDto, confirmDto.ClientRequestId), ct);

                await _uow.BookingDrafts.DeleteAsync(draft, ct);
                await _uow.SaveChangesAsync(ct);

                return new BookingPipelineSubmissionResultDto
                {
                    DraftId = draft.Id,
                    RequiresLogin = false,
                    Booking = booking
                };
            }

            if (string.IsNullOrWhiteSpace(draft.GuestEmail) || string.IsNullOrWhiteSpace(draft.GuestFullName))
            {
                throw new InvalidOperationException("Guest contact details are required.");
            }

            var guestRequest = new GuestBookingRequestDto
            {
                FullName = draft.GuestFullName!,
                Email = draft.GuestEmail!,
                Phone = draft.GuestPhone,
                TechnicianId = draft.TechnicianId.Value,
                ServiceCategoryId = draft.ServiceCategoryId,
                ServiceIssueId = draft.ServiceIssueId,
                Start = draft.SlotStartUtc.Value,
                End = draft.SlotEndUtc.Value,
                PreferredPaymentMethod = draft.PreferredPaymentMethod,
                Notes = draft.Notes,
                Address = new BookingAddressDto
                {
                    Line1 = draft.AddressLine1!,
                    Line2 = draft.AddressLine2,
                    City = draft.City!,
                    State = draft.State,
                    PostalCode = draft.PostalCode!,
                    Country = draft.Country!
                },
                Items = draft.Items.Select(i => new CreateBookingItemDto
                {
                    ServiceIssueId = i.ServiceIssueId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Notes = i.Notes
                }).ToList(),
                ClientRequestId = confirmDto.ClientRequestId
            };

            var guestResult = await _mediator.Send(new CreateGuestBookingCommand(guestRequest), ct);

            if (guestResult.RequiresLogin)
            {
                draft.Status = BookingDraftStatus.Submitted;
                await _uow.BookingDrafts.UpdateAsync(draft, ct);
                await _uow.SaveChangesAsync(ct);

                return new BookingPipelineSubmissionResultDto
                {
                    DraftId = draft.Id,
                    RequiresLogin = true,
                    ExistingUserId = guestResult.ExistingUserId,
                    Booking = guestResult.Booking
                };
            }

            await _uow.BookingDrafts.DeleteAsync(draft, ct);
            await _uow.SaveChangesAsync(ct);

            return new BookingPipelineSubmissionResultDto
            {
                DraftId = draft.Id,
                RequiresLogin = false,
                Booking = guestResult.Booking
            };
        }
    }
}

