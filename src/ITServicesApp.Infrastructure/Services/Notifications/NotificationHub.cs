using Microsoft.AspNetCore.SignalR;

namespace ITServicesApp.Infrastructure.Services.Notifications
{
    public sealed class NotificationHub : Hub
    {
        // Single source of truth for the SignalR client method your frontend subscribes to
        public const string InAppClientMethod = "inapp";
    }
}
