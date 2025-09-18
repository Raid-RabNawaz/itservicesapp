using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public sealed class TechnicianExpertiseRepository : ITechnicianExpertiseRepository
    {
        private readonly ApplicationDbContext _db;

        public TechnicianExpertiseRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(TechnicianExpertise expertise, CancellationToken ct = default)
        {
            await _db.TechnicianExpertises.AddAsync(expertise, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task RemoveAsync(int technicianId, int serviceIssueId, CancellationToken ct = default)
        {
            var entity = await _db.TechnicianExpertises
                .FirstOrDefaultAsync(e => e.TechnicianId == technicianId && e.ServiceIssueId == serviceIssueId, ct);
            if (entity == null) return;
            _db.TechnicianExpertises.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }

        public Task<bool> ExistsAsync(int technicianId, int serviceIssueId, CancellationToken ct = default)
        {
            return _db.TechnicianExpertises
                .AsNoTracking()
                .AnyAsync(e => e.TechnicianId == technicianId && e.ServiceIssueId == serviceIssueId, ct);
        }

        public async Task<IReadOnlyList<TechnicianExpertise>> ListByTechnicianAsync(int technicianId, CancellationToken ct = default)
        {
            return await _db.TechnicianExpertises
                .AsNoTracking()
                .Where(e => e.TechnicianId == technicianId)
                .Include(e => e.ServiceIssue)
                .ToListAsync(ct);
        }
    }
}
