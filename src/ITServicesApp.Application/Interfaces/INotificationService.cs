using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface INotificationService
    {
        Task NotifyUserAsync(int userId, string title, string message, CancellationToken ct = default);

        /// <summary>Persist and fan-out a message to many users.</summary>
        Task NotifyUsersAsync(IEnumerable<int> userIds, string title, string message, CancellationToken ct = default);

        Task<IReadOnlyList<NotificationDto>> ListAsync(int? userId, int take, int skip, CancellationToken ct = default);
        Task<int> CountUnreadAsync(CancellationToken ct = default);
        Task MarkReadAsync(int notificationId, CancellationToken ct = default);
        Task MarkAllReadAsync(CancellationToken ct = default);
    }
}
