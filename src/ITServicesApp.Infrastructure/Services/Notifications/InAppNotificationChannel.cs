using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace ITServicesApp.Infrastructure.Services.Notifications
{
    public class InAppNotificationChannel : INotificationChannel
    {
        public string Name => "inapp";
        private readonly IHubContext<NotificationHub> _hub;

        public InAppNotificationChannel(IHubContext<NotificationHub> hub) => _hub = hub;

        public Task SendAsync(NotificationDto notification, CancellationToken ct = default)
        {
            // Keep payload minimal to match tests/front-end expectations
            var payload = new { notification.Title, notification.Message };

            return _hub.Clients
                       .User(notification.UserId.ToString(CultureInfo.InvariantCulture))
                       .SendCoreAsync(NotificationHub.InAppClientMethod, new object[] { payload }, ct);
        }
    }
}
