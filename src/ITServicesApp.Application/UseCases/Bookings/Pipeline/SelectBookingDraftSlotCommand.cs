using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Pipeline
{
    public sealed record SelectBookingDraftSlotCommand(Guid DraftId, BookingPipelineSlotRequestDto Slot) : IRequest<BookingPipelineStateDto>;

    public sealed class SelectBookingDraftSlotCommandHandler : IRequestHandler<SelectBookingDraftSlotCommand, BookingPipelineStateDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IBookingAssignmentService _assignment;
        private readonly IMapper _mapper;

        public SelectBookingDraftSlotCommandHandler(IUnitOfWork uow, IBookingAssignmentService assignment, IMapper mapper)
        {
            _uow = uow;
            _assignment = assignment;
            _mapper = mapper;
        }

        public async Task<BookingPipelineStateDto> Handle(SelectBookingDraftSlotCommand request, CancellationToken ct)
        {
            var draft = await _uow.BookingDrafts.GetAsync(request.DraftId, ct)
                        ?? throw new InvalidOperationException("Booking draft not found.");

            if (draft.Status != BookingDraftStatus.Pending)
            {
                throw new InvalidOperationException("Only pending drafts can be updated.");
            }

            if (!draft.ServiceIssueId.HasValue)
            {
                throw new InvalidOperationException("Service selection must be completed before choosing a slot.");
            }

            var dto = request.Slot ?? throw new ArgumentNullException(nameof(request.Slot));
            var startUtc = DateTime.SpecifyKind(dto.StartUtc, DateTimeKind.Utc);

            if (startUtc < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Slot must be scheduled in the future.");
            }

            var requestedDuration = dto.DurationMinutes ?? 0;
            if (requestedDuration <= 0 && dto.EndUtc.HasValue && dto.EndUtc.Value > dto.StartUtc)
            {
                requestedDuration = (int)Math.Round((dto.EndUtc.Value - dto.StartUtc).TotalMinutes);
            }

            var durationMinutes = draft.EstimatedDurationMinutes > 0 ? draft.EstimatedDurationMinutes : requestedDuration;
            if (durationMinutes <= 0)
            {
                durationMinutes = 60;
            }

            var endUtc = startUtc.AddMinutes(durationMinutes);

            int technicianId;
            int? slotId = null;

            if (dto.TechnicianId.HasValue)
            {
                technicianId = dto.TechnicianId.Value;
                var isAvailable = await _assignment.IsTechnicianAvailableAsync(technicianId, startUtc, durationMinutes, ct);
                if (!isAvailable)
                {
                    throw new InvalidOperationException("Selected technician is not available for the requested time.");
                }

                var slots = await _uow.TechnicianSlots.GetAvailableAsync(technicianId, startUtc.Date, ct);
                var matchingSlot = slots.FirstOrDefault(s => s.StartUtc <= startUtc && endUtc <= s.EndUtc);
                slotId = matchingSlot?.Id;
            }
            else
            {
                var suggestion = await _assignment.FindBestAsync(draft.ServiceCategoryId ?? 0, draft.ServiceIssueId.Value, startUtc, durationMinutes, ct);
                if (suggestion == null)
                {
                    throw new InvalidOperationException("No technicians are available for the requested time.");
                }

                technicianId = suggestion.TechnicianId;
                slotId = suggestion.SlotId;
                endUtc = suggestion.EndUtc;
            }

            draft.TechnicianId = technicianId;
            draft.SlotStartUtc = startUtc;
            draft.SlotEndUtc = endUtc;
            draft.SlotId = slotId;
            draft.EstimatedDurationMinutes = durationMinutes;

            await _uow.BookingDrafts.UpdateAsync(draft, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<BookingPipelineStateDto>(draft);
        }
    }
}
