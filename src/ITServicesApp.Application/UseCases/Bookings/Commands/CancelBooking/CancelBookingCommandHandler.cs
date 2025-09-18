using System;
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

namespace ITServicesApp.Application.UseCases.Bookings.Commands.CancelBooking
{
    public sealed class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, BookingDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _clock;
        private readonly IBackgroundJobService _jobs;
        private readonly IMediator _mediator;

        public CancelBookingCommandHandler(
            IUnitOfWork uow,
            IMapper mapper,
            IDateTimeProvider clock,
            IBackgroundJobService jobs,
            IMediator mediator)
        {
            _uow = uow; _mapper = mapper; _clock = clock; _jobs = jobs; _mediator = mediator;
        }

        public async Task<BookingDto> Handle(CancelBookingCommand request, CancellationToken ct)
        {
            var b = await _uow.Bookings.GetByIdAsync(request.BookingId, ct)
                    ?? throw new InvalidOperationException("Booking not found.");

            if (b.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("Booking already cancelled.");

            if (b.ScheduledStartUtc - _clock.UtcNow < TimeSpan.FromHours(24))
                throw new InvalidOperationException("Cannot cancel within 24 hours of the visit.");

            // cancel reminder if any
            if (!string.IsNullOrWhiteSpace(b.ReminderJobId))
            {
                await _jobs.CancelBookingReminderAsync(b.ReminderJobId!, ct);
                b.ReminderJobId = null;
            }

            b.Status = BookingStatus.Cancelled;
            _uow.Bookings.Update(b);
            await _uow.SaveChangesAsync(ct);

            // ⬇️ pass all required ctor args: (BookingId, UserId, TechnicianId, ScheduledStartUtc)
            await _mediator.Publish(new BookingCancelledDomainEvent(b.Id, b.UserId, b.TechnicianId, b.ScheduledStartUtc), ct);

            return _mapper.Map<BookingDto>(b);
        }
    }
}
