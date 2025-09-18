using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public class ServiceReportRepository : IServiceReportRepository
    {
        private readonly ApplicationDbContext _db;
        public ServiceReportRepository(ApplicationDbContext db) => _db = db;
        public Task<ServiceReport?> GetByIdAsync(int id, CancellationToken ct = default) => _db.ServiceReports.FindAsync(new object?[] { id }, ct).AsTask();
        public Task<IReadOnlyList<ServiceReport>> ListAsync(System.Linq.Expressions.Expression<System.Func<ServiceReport, bool>>? predicate = null, CancellationToken ct = default)
        { var q = _db.ServiceReports.AsQueryable(); if (predicate != null) q = q.Where(predicate); return q.ToListAsync(ct).ContinueWith(t => (IReadOnlyList<ServiceReport>)t.Result!, ct); }
        public Task AddAsync(ServiceReport entity, CancellationToken ct = default) => _db.ServiceReports.AddAsync(entity, ct).AsTask();
        public void Update(ServiceReport entity) => _db.ServiceReports.Update(entity);
        public void Delete(ServiceReport entity) => _db.ServiceReports.Remove(entity);
        public Task<ServiceReport?> GetByBookingAsync(int bookingId, CancellationToken ct) => _db.ServiceReports.FirstOrDefaultAsync(x => x.BookingId == bookingId, ct);
    }
}
