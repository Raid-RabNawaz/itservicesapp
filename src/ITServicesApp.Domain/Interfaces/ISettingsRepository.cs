
using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface ISettingsRepository : IRepository<PlatformSettings>
    {
        Task<PlatformSettings> GetSingletonAsync(CancellationToken ct);
    }
}