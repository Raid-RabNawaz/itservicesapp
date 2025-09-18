using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface IAddressRepository : IRepository<Address>
    {
        Task<Address?> GetDefaultAsync(int userId, CancellationToken ct);
        Task<IReadOnlyList<Address>> ListByUserAsync(int userId, int take, int skip, CancellationToken ct);
    }
}