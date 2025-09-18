using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationDbContext _db;
        public MessageRepository(ApplicationDbContext db) => _db = db;
        public Task<MessageThread?> GetThreadByBookingAsync(int bookingId, CancellationToken ct)
            => _db.MessageThreads.Include(t => t.Messages).FirstOrDefaultAsync(t => t.BookingId == bookingId, ct);
        public async Task<MessageThread> AddThreadAsync(MessageThread thread, CancellationToken ct)
        { await _db.MessageThreads.AddAsync(thread, ct); await _db.SaveChangesAsync(ct); return thread; }
        public async Task<Message> AddMessageAsync(Message message, CancellationToken ct)
        { await _db.Messages.AddAsync(message, ct); await _db.SaveChangesAsync(ct); return message; }
        public async Task<IReadOnlyList<Message>> ListMessagesAsync(int threadId, int take, int skip, CancellationToken ct)
            => await _db.Messages.Where(m => m.ThreadId == threadId).OrderBy(m => m.SentAtUtc).Skip(skip).Take(take).ToListAsync(ct);
    }
}
