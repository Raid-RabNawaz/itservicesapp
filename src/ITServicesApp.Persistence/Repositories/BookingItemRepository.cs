using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public sealed class BookingItemRepository : GenericRepository<BookingItem>, IBookingItemRepository
    {
        public BookingItemRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<IReadOnlyList<BookingItem>> ListByBookingAsync(int bookingId, CancellationToken ct = default)
        {
            return await _db.BookingItems
                .Where(i => i.BookingId == bookingId)
                .AsNoTracking()
                .Include(i => i.ServiceIssue)
                .ToListAsync(ct);
        }

        public async Task DeleteByBookingAsync(int bookingId, CancellationToken ct = default)
        {
            var items = await _db.BookingItems.Where(i => i.BookingId == bookingId).ToListAsync(ct);
            if (items.Count == 0) return;
            _db.BookingItems.RemoveRange(items);
            await _db.SaveChangesAsync(ct);
        }
    }
}
