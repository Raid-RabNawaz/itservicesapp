using System;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Events;
using ITServicesApp.Domain.Interfaces;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Events
{
    public sealed class BookingCreatedDomainEventHandler : INotificationHandler<BookingCreatedDomainEvent>
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notify;
        private readonly IBackgroundJobService _jobs;
        private readonly IDateTimeProvider _clock;

        public BookingCreatedDomainEventHandler(IUnitOfWork uow, INotificationService notify, IBackgroundJobService jobs, IDateTimeProvider clock)
        {
            _uow = uow; _notify = notify; _jobs = jobs; _clock = clock;
        }

        public async Task Handle(BookingCreatedDomainEvent notification, CancellationToken ct)
        {
            var booking = await _uow.Bookings.GetByIdAsync(notification.BookingId, ct);
            if (booking is null) return;

            // Notify user
            await _notify.NotifyUserAsync(booking.UserId, "Booking confirmed",
                $"Your visit is scheduled at {booking.ScheduledStartUtc:yyyy-MM-dd HH:mm} UTC.", ct);

            // Notify technician user (via Technician -> UserId)
            var techUserId = (await _uow.Technicians.GetByIdAsync(booking.TechnicianId, ct))?.UserId;
            if (techUserId is int tid)
            {
                await _notify.NotifyUserAsync(tid, "New booking assigned",
                    $"A booking on {booking.ScheduledStartUtc:yyyy-MM-dd HH:mm} UTC has been assigned to you.", ct);
            }

            // Schedule reminder ~2h before start if still in the future
            var remindAt = booking.ScheduledStartUtc.AddHours(-2);
            if (remindAt > _clock.UtcNow)
                await _jobs.ScheduleBookingReminderAsync(booking.Id, remindAt, ct);
        }
    }
}
