using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface IServiceCategoryRepository : IRepository<ServiceCategory>
    {
        Task<IReadOnlyList<ServiceCategory>> ListAllAsync(CancellationToken ct = default);
        Task<bool> HasIssuesAsync(int categoryId, CancellationToken ct = default);
    }
}
