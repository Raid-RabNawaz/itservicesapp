using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface ITechnicianUnavailabilityRepository : IRepository<TechnicianUnavailability>
    {
        Task<bool> HasOverlapAsync(int technicianId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
        Task<List<TechnicianUnavailability>> ListForTechnicianAsync(int technicianId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
    }
}
