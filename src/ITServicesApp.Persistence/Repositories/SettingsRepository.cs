using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly ApplicationDbContext _db;
        public SettingsRepository(ApplicationDbContext db) => _db = db;
        public Task<PlatformSettings?> GetByIdAsync(int id, CancellationToken ct = default) => _db.PlatformSettings.FindAsync(new object?[] { id }, ct).AsTask();
        public Task<IReadOnlyList<PlatformSettings>> ListAsync(System.Linq.Expressions.Expression<System.Func<PlatformSettings, bool>>? predicate = null, CancellationToken ct = default)
        { var q = _db.PlatformSettings.AsQueryable(); if (predicate != null) q = q.Where(predicate); return q.ToListAsync(ct).ContinueWith(t => (IReadOnlyList<PlatformSettings>)t.Result!, ct); }
        public Task AddAsync(PlatformSettings entity, CancellationToken ct = default) => _db.PlatformSettings.AddAsync(entity, ct).AsTask();
        public void Update(PlatformSettings entity) => _db.PlatformSettings.Update(entity);
        public void Delete(PlatformSettings entity) => _db.PlatformSettings.Remove(entity);
        public async Task<PlatformSettings> GetSingletonAsync(CancellationToken ct)
        {
            var row = await _db.PlatformSettings.FirstOrDefaultAsync(ct);
            if (row == null)
            {
                row = new PlatformSettings
                {
                    TechnicianCommissionRate = 0.10m,
                    CancellationPolicyHours = 24,
                    Currency = "USD",
                    ModifiedAtUtc = System.DateTime.UtcNow
                };
                await _db.PlatformSettings.AddAsync(row, ct);
                await _db.SaveChangesAsync(ct);
            }
            else if (row.TechnicianCommissionRate <= 0m || row.TechnicianCommissionRate >= 1m)
            {
                row.TechnicianCommissionRate = 0.10m;
                row.ModifiedAtUtc = System.DateTime.UtcNow;
                _db.PlatformSettings.Update(row);
                await _db.SaveChangesAsync(ct);
            }
            return row;
        }
    }
}
