using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Events;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.CompleteBooking
{
    public sealed class CompleteBookingCommandHandler : IRequestHandler<CompleteBookingCommand, BookingDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IBackgroundJobService _jobs;
        private readonly IMediator _mediator;

        public CompleteBookingCommandHandler(IUnitOfWork uow, IMapper mapper, IBackgroundJobService jobs, IMediator mediator)
        {
            _uow = uow; _mapper = mapper; _jobs = jobs; _mediator = mediator;
        }

        public async Task<BookingDto> Handle(CompleteBookingCommand request, CancellationToken ct)
        {
            var b = await _uow.Bookings.GetByIdAsync(request.BookingId, ct)
                    ?? throw new InvalidOperationException("Booking not found.");
            if (b.Status == BookingStatus.Cancelled) throw new InvalidOperationException("Cancelled booking cannot be completed.");
            if (b.Status == BookingStatus.Completed) return _mapper.Map<BookingDto>(b);

            if (request.ActualEndUtc.HasValue && request.ActualEndUtc.Value >= b.ScheduledStartUtc)
                b.ScheduledEndUtc = request.ActualEndUtc.Value;

            // Cancel reminder if still scheduled
            if (!string.IsNullOrWhiteSpace(b.ReminderJobId))
            {
                await _jobs.CancelBookingReminderAsync(b.ReminderJobId!, ct);
                b.ReminderJobId = null;
            }

            b.Status = BookingStatus.Completed;
            _uow.Bookings.Update(b);
            await _uow.SaveChangesAsync(ct);

            await _mediator.Publish(new BookingCompletedDomainEvent(b.Id), ct);
            return _mapper.Map<BookingDto>(b);
        }
    }
}
