using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface ITechnicianSlotRepository : IRepository<TechnicianSlot>
    {
        Task<IReadOnlyList<TechnicianSlot>> GetAvailableAsync(int technicianId, DateTime date, CancellationToken ct = default);
        Task<TechnicianSlot?> GetByTechAndStartAsync(int technicianId, DateTime startUtc, CancellationToken ct = default);
    }
}
