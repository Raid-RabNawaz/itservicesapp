using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly ApplicationDbContext _db;
        public PasswordResetRepository(ApplicationDbContext db) => _db = db;
        public Task<PasswordResetToken?> GetByIdAsync(int id, CancellationToken ct = default) => _db.PasswordResetTokens.FindAsync(new object?[] { id }, ct).AsTask();
        public Task<IReadOnlyList<PasswordResetToken>> ListAsync(System.Linq.Expressions.Expression<System.Func<PasswordResetToken, bool>>? predicate = null, CancellationToken ct = default)
        { var q = _db.PasswordResetTokens.AsQueryable(); if (predicate != null) q = q.Where(predicate); return q.ToListAsync(ct).ContinueWith(t => (IReadOnlyList<PasswordResetToken>)t.Result!, ct); }
        public Task AddAsync(PasswordResetToken entity, CancellationToken ct = default) => _db.PasswordResetTokens.AddAsync(entity, ct).AsTask();
        public void Update(PasswordResetToken entity) => _db.PasswordResetTokens.Update(entity);
        public void Delete(PasswordResetToken entity) => _db.PasswordResetTokens.Remove(entity);
        public Task<PasswordResetToken?> FindValidAsync(int userId, string token, CancellationToken ct)
            => _db.PasswordResetTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.Token == token && x.UsedAtUtc == null && x.ExpiresAtUtc > System.DateTime.UtcNow, ct);
    }
}
