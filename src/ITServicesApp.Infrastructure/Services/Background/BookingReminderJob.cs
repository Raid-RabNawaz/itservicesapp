using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Infrastructure.Services.Background
{
    public class BookingReminderJob
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notifier;

        public BookingReminderJob(IUnitOfWork uow, INotificationService notifier)
        {
            _uow = uow;
            _notifier = notifier;
        }

        public async Task RunAsync(int bookingId, CancellationToken ct = default)
        {
            var booking = await _uow.Bookings.GetByIdAsync(bookingId, ct);
            if (booking == null) return;

            await _notifier.NotifyUserAsync(booking.UserId, "Upcoming booking reminder",
                $"Your technician visit is scheduled at {booking.ScheduledStartUtc:yyyy-MM-dd HH:mm} UTC.", ct);
        }
    }
}
