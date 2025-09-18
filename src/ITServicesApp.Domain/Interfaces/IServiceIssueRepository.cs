using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface IServiceIssueRepository : IRepository<ServiceIssue>
    {
        Task<IReadOnlyList<ServiceIssue>> ListByCategoryAsync(int categoryId, CancellationToken ct = default);
    }
}
