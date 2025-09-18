using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces.Notifications
{
    public interface INotificationChannel
    {
        string Name { get; }

        /// <summary>
        /// Send a single notification to its intended recipient.
        /// </summary>
        Task SendAsync(NotificationDto notification, CancellationToken ct = default);
    }
}
