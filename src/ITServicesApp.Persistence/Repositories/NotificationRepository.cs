using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public sealed class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext db) : base(db) { }

        public async Task<IReadOnlyList<Notification>> ListByUserAsync(int userId, int take, int skip, CancellationToken ct = default)
        {
            var list = await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAtUtc)
                .Skip(skip).Take(take)
                .AsNoTracking()
                .ToListAsync(ct);
            return list;
        }

        public Task<int> CountUnreadAsync(int userId, CancellationToken ct = default)
            => _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);

        public Task<List<Notification>> ListUnreadAsync(int userId, CancellationToken ct = default)
            => _db.Notifications.AsNoTracking()
                 .Where(n => n.UserId == userId && !n.IsRead)
                 .OrderByDescending(n => n.CreatedAtUtc)
                 .ToListAsync(ct);
    }
}
