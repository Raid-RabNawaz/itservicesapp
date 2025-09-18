
using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface INotificationTemplateRepository : IRepository<NotificationTemplate>
    {
        Task<NotificationTemplate?> GetByKeyAsync(string key, CancellationToken ct);
    }
}