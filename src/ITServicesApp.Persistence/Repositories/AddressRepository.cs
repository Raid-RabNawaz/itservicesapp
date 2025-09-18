using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly ApplicationDbContext _db;
        public AddressRepository(ApplicationDbContext db) => _db = db;
        public Task<Address?> GetByIdAsync(int id, CancellationToken ct = default) => _db.Addresses.FindAsync(new object?[] { id }, ct).AsTask();
        public Task<IReadOnlyList<Address>> ListAsync(System.Linq.Expressions.Expression<System.Func<Address, bool>>? predicate = null, CancellationToken ct = default)
        {
            var q = _db.Addresses.AsQueryable();
            if (predicate != null) q = q.Where(predicate);
            return q.ToListAsync(ct).ContinueWith(t => (IReadOnlyList<Address>)t.Result!, ct);
        }
        public Task AddAsync(Address entity, CancellationToken ct = default) => _db.Addresses.AddAsync(entity, ct).AsTask();
        public void Update(Address entity) => _db.Addresses.Update(entity);
        public void Delete(Address entity) => _db.Addresses.Remove(entity);
        public Task<Address?> GetDefaultAsync(int userId, CancellationToken ct) => _db.Addresses.FirstOrDefaultAsync(x => x.UserId == userId && x.IsDefault, ct);
        public async Task<IReadOnlyList<Address>> ListByUserAsync(int userId, int take, int skip, CancellationToken ct)
            => await _db.Addresses.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAtUtc).Skip(skip).Take(take).ToListAsync(ct);
    }
}
