using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IReadOnlyList<Notification>> ListByUserAsync(int userId, int take, int skip, CancellationToken ct = default);
        Task<int> CountUnreadAsync(int userId, CancellationToken ct = default);
        Task<List<Notification>> ListUnreadAsync(int userId, CancellationToken ct = default);
    }
}
