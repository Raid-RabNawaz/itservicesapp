using System.Linq.Expressions;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    // Implements your ITServicesApp.Domain.Interfaces.IRepository<T>
    public abstract class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _db;
        protected readonly DbSet<T> _set;

        protected GenericRepository(ApplicationDbContext db)
        {
            _db = db;
            _set = db.Set<T>();
        }

        public virtual Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
            => _set.FindAsync(new object[] { id }, ct).AsTask();

        public virtual async Task<IReadOnlyList<T>> ListAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken ct = default)
        {
            var q = _set.AsQueryable();
            if (predicate != null) q = q.Where(predicate);
            return await q.ToListAsync(ct);
        }

        public virtual Task AddAsync(T entity, CancellationToken ct = default)
            => _set.AddAsync(entity, ct).AsTask();

        public virtual void Update(T entity) => _set.Update(entity);
        public virtual void Delete(T entity) => _set.Remove(entity);
    }
}
