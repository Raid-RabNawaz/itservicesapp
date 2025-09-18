using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext db) : base(db) { }

        public Task<Booking?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.Bookings
                  .Include(b => b.User)
                  .Include(b => b.Technician).ThenInclude(t => t.User)
                  .Include(b => b.ServiceCategory)
                  .Include(b => b.ServiceIssue)
                  .Include(b => b.Items).ThenInclude(i => i.ServiceIssue)
                  .Include(b => b.Payment)
                  .Include(b => b.AddressEntity)
                  .FirstOrDefaultAsync(b => b.Id == id, ct);

        public Task<bool> HasOverlapAsync(int technicianId, DateTime startUtc, DateTime endUtc, CancellationToken ct)
            => _db.Bookings
                  .AsNoTracking()
                  .Where(b => b.TechnicianId == technicianId && b.Status != BookingStatus.Cancelled)
                  .AnyAsync(b => b.ScheduledStartUtc < endUtc && startUtc < b.ScheduledEndUtc, ct);

        public Task<List<Booking>> ListForUserAsync(int userId, CancellationToken ct = default)
            => _db.Bookings
                  .AsNoTracking()
                  .Where(b => b.UserId == userId)
                  .OrderByDescending(b => b.ScheduledStartUtc)
                  .Include(b => b.Items)
                  .Include(b => b.Payment)
                  .ToListAsync(ct);
    }
}
