using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public class NotificationTemplateRepository : INotificationTemplateRepository
    {
        private readonly ApplicationDbContext _db;
        public NotificationTemplateRepository(ApplicationDbContext db) => _db = db;
        public Task<NotificationTemplate?> GetByIdAsync(int id, CancellationToken ct = default) => _db.NotificationTemplates.FindAsync(new object?[] { id }, ct).AsTask();
        public Task<IReadOnlyList<NotificationTemplate>> ListAsync(System.Linq.Expressions.Expression<System.Func<NotificationTemplate, bool>>? predicate = null, CancellationToken ct = default)
        { var q = _db.NotificationTemplates.AsQueryable(); if (predicate != null) q = q.Where(predicate); return q.ToListAsync(ct).ContinueWith(t => (IReadOnlyList<NotificationTemplate>)t.Result!, ct); }
        public Task AddAsync(NotificationTemplate entity, CancellationToken ct = default) => _db.NotificationTemplates.AddAsync(entity, ct).AsTask();
        public void Update(NotificationTemplate entity) => _db.NotificationTemplates.Update(entity);
        public void Delete(NotificationTemplate entity) => _db.NotificationTemplates.Remove(entity);
        public Task<NotificationTemplate?> GetByKeyAsync(string key, CancellationToken ct) => _db.NotificationTemplates.FirstOrDefaultAsync(x => x.Key == key, ct);
    }
}
