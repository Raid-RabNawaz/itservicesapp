using System.Linq.Expressions;

namespace ITServicesApp.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Generic, PK-type-agnostic
        Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
        Task AddAsync(T entity, CancellationToken ct = default);
        void Update(T entity);
        void Delete(T entity);
    }
}
