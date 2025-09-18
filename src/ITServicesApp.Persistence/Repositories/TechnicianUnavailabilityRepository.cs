using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public sealed class TechnicianUnavailabilityRepository : GenericRepository<TechnicianUnavailability>, ITechnicianUnavailabilityRepository
    {
        public TechnicianUnavailabilityRepository(ApplicationDbContext db) : base(db) { }

        public Task<bool> HasOverlapAsync(int technicianId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
            => _db.TechnicianUnavailabilities
                   .AnyAsync(u => u.TechnicianId == technicianId && u.StartUtc < endUtc && startUtc < u.EndUtc, ct);

        public Task<List<TechnicianUnavailability>> ListForTechnicianAsync(int technicianId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
            => _db.TechnicianUnavailabilities.AsNoTracking()
                 .Where(u => u.TechnicianId == technicianId && u.StartUtc < toUtc && fromUtc < u.EndUtc)
                 .OrderBy(u => u.StartUtc)
                 .ToListAsync(ct);

    }
}
