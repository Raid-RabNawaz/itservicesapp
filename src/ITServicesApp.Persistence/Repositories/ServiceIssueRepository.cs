using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public sealed class ServiceIssueRepository : GenericRepository<ServiceIssue>, IServiceIssueRepository
    {
        public ServiceIssueRepository(ApplicationDbContext db) : base(db) { }

        public async Task<IReadOnlyList<ServiceIssue>> ListByCategoryAsync(int categoryId, CancellationToken ct = default)
        {
            var list = await _db.ServiceIssues
                .Where(i => i.ServiceCategoryId == categoryId)
                .AsNoTracking()
                .ToListAsync(ct);
            return list;
        }
    }
}
