using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public sealed class ServiceCategoryRepository : GenericRepository<ServiceCategory>, IServiceCategoryRepository
    {
        public ServiceCategoryRepository(ApplicationDbContext db) : base(db) { }

        public async Task<IReadOnlyList<ServiceCategory>> ListAllAsync(CancellationToken ct = default)
        {
            var list = await _db.ServiceCategories.AsNoTracking().ToListAsync(ct);
            return list;
        }

        public Task<bool> HasIssuesAsync(int categoryId, CancellationToken ct = default)
            => _db.ServiceIssues.AsNoTracking().AnyAsync(i => i.ServiceCategoryId == categoryId, ct);

    }
}
