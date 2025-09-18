using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ApplicationDbContext _db;
        public InvoiceRepository(ApplicationDbContext db) => _db = db;
        public Task<Invoice?> GetByIdAsync(int id, CancellationToken ct = default) => _db.Invoices.FindAsync(new object?[] { id }, ct).AsTask();
        public Task<IReadOnlyList<Invoice>> ListAsync(System.Linq.Expressions.Expression<System.Func<Invoice, bool>>? predicate = null, CancellationToken ct = default)
        { var q = _db.Invoices.AsQueryable(); if (predicate != null) q = q.Where(predicate); return q.ToListAsync(ct).ContinueWith(t => (IReadOnlyList<Invoice>)t.Result!, ct); }
        public Task AddAsync(Invoice entity, CancellationToken ct = default) => _db.Invoices.AddAsync(entity, ct).AsTask();
        public void Update(Invoice entity) => _db.Invoices.Update(entity);
        public void Delete(Invoice entity) => _db.Invoices.Remove(entity);
        public Task<Invoice?> GetByBookingAsync(int bookingId, CancellationToken ct) => _db.Invoices.FirstOrDefaultAsync(x => x.BookingId == bookingId, ct);
    }
}
