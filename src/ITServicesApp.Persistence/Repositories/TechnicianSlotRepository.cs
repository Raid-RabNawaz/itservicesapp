using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public sealed class TechnicianSlotRepository : GenericRepository<TechnicianSlot>, ITechnicianSlotRepository
    {
        public TechnicianSlotRepository(ApplicationDbContext db) : base(db) { }

        public async Task<IReadOnlyList<TechnicianSlot>> GetAvailableAsync(int technicianId, DateTime date, CancellationToken ct = default)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(14);

            var list = await _db.TechnicianSlots
                .Where(s => s.TechnicianId == technicianId
                            && s.EndUtc > dayStart
                            && s.StartUtc < dayEnd)
                .OrderBy(s => s.StartUtc)
                .ToListAsync(ct);
            return list;
        }

        public Task<TechnicianSlot?> GetByTechAndStartAsync(int technicianId, DateTime startUtc, CancellationToken ct = default)
            => _db.TechnicianSlots.FirstOrDefaultAsync(s => s.TechnicianId == technicianId && s.StartUtc == startUtc, ct);
    }
}
