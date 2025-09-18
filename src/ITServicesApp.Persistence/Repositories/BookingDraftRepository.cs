using System;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public sealed class BookingDraftRepository : IBookingDraftRepository
    {
        private readonly ApplicationDbContext _db;

        public BookingDraftRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<BookingDraft?> GetAsync(Guid id, CancellationToken ct = default)
            => _db.BookingDrafts
                   .Include(d => d.Items)
                   .FirstOrDefaultAsync(d => d.Id == id, ct);

        public Task AddAsync(BookingDraft draft, CancellationToken ct = default)
            => _db.BookingDrafts.AddAsync(draft, ct).AsTask();

        public Task UpdateAsync(BookingDraft draft, CancellationToken ct = default)
        {
            _db.BookingDrafts.Update(draft);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(BookingDraft draft, CancellationToken ct = default)
        {
            _db.BookingDrafts.Remove(draft);
            return Task.CompletedTask;
        }
    }
}
