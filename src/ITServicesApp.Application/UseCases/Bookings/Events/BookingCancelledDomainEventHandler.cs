using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Events;
using ITServicesApp.Domain.Interfaces;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Events
{
    public sealed class BookingCancelledDomainEventHandler : INotificationHandler<BookingCancelledDomainEvent>
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notify;

        public BookingCancelledDomainEventHandler(IUnitOfWork uow, INotificationService notify)
        {
            _uow = uow; _notify = notify;
        }

        public async Task Handle(BookingCancelledDomainEvent notification, CancellationToken ct)
        {
            var booking = await _uow.Bookings.GetByIdAsync(notification.BookingId, ct);
            if (booking is null) return;

            await _notify.NotifyUserAsync(booking.UserId, "Booking cancelled",
                $"Your booking for {booking.ScheduledStartUtc:yyyy-MM-dd HH:mm} UTC was cancelled.", ct);

            var techUserId = (await _uow.Technicians.GetByIdAsync(booking.TechnicianId, ct))?.UserId;
            if (techUserId is int tid)
                await _notify.NotifyUserAsync(tid, "Booking cancelled",
                    $"A booking on {booking.ScheduledStartUtc:yyyy-MM-dd HH:mm} UTC was cancelled.", ct);
        }
    }
}
