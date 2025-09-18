using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Events;
using ITServicesApp.Domain.Interfaces;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Events
{
    public sealed class BookingCompletedDomainEventHandler : INotificationHandler<BookingCompletedDomainEvent>
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notify;

        public BookingCompletedDomainEventHandler(IUnitOfWork uow, INotificationService notify)
        {
            _uow = uow; _notify = notify;
        }

        public async Task Handle(BookingCompletedDomainEvent notification, CancellationToken ct)
        {
            var booking = await _uow.Bookings.GetByIdAsync(notification.BookingId, ct);
            if (booking is null) return;

            await _notify.NotifyUserAsync(booking.UserId, "Thanks for using our service",
                "Your booking was marked completed. Please consider leaving a review for your technician.", ct);
        }
    }
}
