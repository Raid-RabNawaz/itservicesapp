using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public sealed class TechnicianReviewRepository : GenericRepository<TechnicianReview>, ITechnicianReviewRepository
    {
        public TechnicianReviewRepository(ApplicationDbContext db) : base(db) { }

        public Task<bool> ExistsForBookingAsync(int bookingId, CancellationToken ct = default)
            => _db.TechnicianReviews.AnyAsync(r => r.BookingId == bookingId, ct);

        public async Task<IReadOnlyList<TechnicianReview>> ListByTechnicianAsync(int technicianId, int take, int skip, CancellationToken ct = default)
        {
            var list = await _db.TechnicianReviews
                .Where(r => r.TechnicianId == technicianId)
                .OrderByDescending(r => r.CreatedAtUtc)
                .Skip(skip).Take(take)
                .AsNoTracking()
                .ToListAsync(ct);
            return list;
        }

        public async Task<double> GetAverageRatingAsync(int technicianId, CancellationToken ct = default)
        {
            var q = _db.TechnicianReviews.AsNoTracking().Where(r => r.TechnicianId == technicianId);
            if (!await q.AnyAsync(ct)) return 0d;
            return await q.AverageAsync(r => r.Rating, ct);
        }
        
    }
}
