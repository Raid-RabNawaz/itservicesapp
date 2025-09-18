using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public class TechnicianRepository : GenericRepository<Technician>, ITechnicianRepository
    {
        private readonly ApplicationDbContext _db;
        public TechnicianRepository(ApplicationDbContext db) : base(db) => _db = db;

        public async Task<List<int>> QueryQualifiedTechnicianIdsAsync(int serviceCategoryId, int serviceIssueId, CancellationToken ct)
        {
            // Simplified: by category; extend to a skills matrix if needed.
            return await _db.Technicians
                .AsNoTracking()
                .Where(t => t.IsActive && t.ServiceCategoryId == serviceCategoryId)
                .Select(t => t.Id)
                .ToListAsync(ct);
        }

        public async Task<bool> AnyFreeAsync(IEnumerable<int> technicianIds, DateTime start, DateTime end, CancellationToken ct)
        {
            var ids = technicianIds.ToArray();
            if (ids.Length == 0) return false;

            var slotTechs =
                from ts in _db.TechnicianSlots
                where ids.Contains(ts.TechnicianId)
                   && ts.StartUtc <= start
                   && end <= ts.EndUtc
                select ts.TechnicianId;

            var busyFromBookings =
                from b in _db.Bookings
                where ids.Contains(b.TechnicianId)
                   && b.Status != BookingStatus.Cancelled
                   && b.ScheduledStartUtc < end && start < b.ScheduledEndUtc
                select b.TechnicianId;

            var busyFromUnavailability =
                from u in _db.TechnicianUnavailabilities
                where ids.Contains(u.TechnicianId)
                   && u.StartUtc < end && start < u.EndUtc
                select u.TechnicianId;

            var busy = busyFromBookings.Union(busyFromUnavailability);

            return await slotTechs.Except(busy).AnyAsync(ct);
        }

        public Task<Technician?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.Technicians
                  .Include(t => t.User)
                  .Include(t => t.Slots)
                  .Include(t => t.Unavailabilities)
                  .FirstOrDefaultAsync(t => t.Id == id, ct);
    }
}
