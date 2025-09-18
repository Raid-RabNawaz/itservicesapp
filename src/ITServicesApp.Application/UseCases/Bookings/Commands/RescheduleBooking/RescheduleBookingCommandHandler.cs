using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Events;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.RescheduleBooking
{
    public sealed class RescheduleBookingCommandHandler : IRequestHandler<RescheduleBookingCommand, BookingDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _clock;
        private readonly IBackgroundJobService _jobs;
        private readonly IMediator _mediator;

        public RescheduleBookingCommandHandler(
            IUnitOfWork uow,
            IMapper mapper,
            IDateTimeProvider clock,
            IBackgroundJobService jobs,
            IMediator mediator)
        {
            _uow = uow; _mapper = mapper; _clock = clock; _jobs = jobs; _mediator = mediator;
        }

        public async Task<BookingDto> Handle(RescheduleBookingCommand request, CancellationToken ct)
        {
            var b = await _uow.Bookings.GetByIdAsync(request.BookingId, ct)
                    ?? throw new InvalidOperationException("Booking not found.");
            if (b.Status == BookingStatus.Cancelled) throw new InvalidOperationException("Cannot reschedule a cancelled booking.");
            if (request.NewStartUtc >= request.NewEndUtc) throw new InvalidOperationException("Start must be before End.");
            if (request.NewStartUtc < _clock.UtcNow) throw new InvalidOperationException("Cannot reschedule to the past.");

            var newTechId = request.NewTechnicianId ?? b.TechnicianId;

            var daySlots = await _uow.TechnicianSlots.GetAvailableAsync(newTechId, request.NewStartUtc.Date, ct);
            if (!daySlots.Any(s => s.StartUtc <= request.NewStartUtc && request.NewEndUtc <= s.EndUtc))
                throw new InvalidOperationException("Technician has no slot covering the new time.");

            if (await _uow.Bookings.HasOverlapAsync(newTechId, request.NewStartUtc, request.NewEndUtc, ct))
                throw new InvalidOperationException("Technician is already booked in that time.");

            // cancel previous reminder
            if (!string.IsNullOrWhiteSpace(b.ReminderJobId))
            {
                await _jobs.CancelBookingReminderAsync(b.ReminderJobId!, ct);
                b.ReminderJobId = null;
            }

            b.TechnicianId = newTechId;
            b.ScheduledStartUtc = request.NewStartUtc;
            b.ScheduledEndUtc = request.NewEndUtc;

            // schedule a new reminder
            var remindAt = b.ScheduledStartUtc.AddHours(-2);
            if (remindAt > _clock.UtcNow)
                b.ReminderJobId = await _jobs.ScheduleBookingReminderAsync(b.Id, remindAt, ct);

            _uow.Bookings.Update(b);
            await _uow.SaveChangesAsync(ct);

            // ⬇️ pass all required ctor args: (BookingId, UserId, TechnicianId, ScheduledStartUtc, Reason)
            await _mediator.Publish(new BookingUpdatedDomainEvent(b.Id, b.UserId, b.TechnicianId, b.ScheduledStartUtc, "Rescheduled"), ct);

            return _mapper.Map<BookingDto>(b);
        }
    }
}
