using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Events;
using ITServicesApp.Domain.Interfaces;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Events
{
    public sealed class BookingUpdatedDomainEventHandler : INotificationHandler<BookingUpdatedDomainEvent>
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notify;
        private readonly IBackgroundJobService _jobs;
        private readonly IDateTimeProvider _clock;

        public BookingUpdatedDomainEventHandler(IUnitOfWork uow, INotificationService notify, IBackgroundJobService jobs, IDateTimeProvider clock)
        {
            _uow = uow; _notify = notify; _jobs = jobs; _clock = clock;
        }

        public async Task Handle(BookingUpdatedDomainEvent notification, CancellationToken ct)
        {
            var booking = await _uow.Bookings.GetByIdAsync(notification.BookingId, ct);
            if (booking is null) return;

            // Notify both parties about the change
            await _notify.NotifyUserAsync(booking.UserId, "Booking updated",
                $"Your booking was updated to {booking.ScheduledStartUtc:yyyy-MM-dd HH:mm} UTC.", ct);

            var techUserId = (await _uow.Technicians.GetByIdAsync(booking.TechnicianId, ct))?.UserId;
            if (techUserId is int tid)
                await _notify.NotifyUserAsync(tid, "Booking updated",
                    $"A booking was updated to {booking.ScheduledStartUtc:yyyy-MM-dd HH:mm} UTC.", ct);

            // Optional: re-schedule reminder (requires cancel support in job system to avoid duplicates)
            var remindAt = booking.ScheduledStartUtc.AddHours(-2);
            if (remindAt > _clock.UtcNow)
                await _jobs.ScheduleBookingReminderAsync(booking.Id, remindAt, ct);
        }
    }
}
