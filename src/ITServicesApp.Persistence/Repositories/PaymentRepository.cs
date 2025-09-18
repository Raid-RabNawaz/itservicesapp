using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        private readonly ApplicationDbContext _db;
        public PaymentRepository(ApplicationDbContext db) : base(db) => _db = db;

        public Task<Payment?> GetByProviderPaymentIdAsync(string providerPaymentId, CancellationToken ct)
            => _db.Payments.FirstOrDefaultAsync(p => p.ProviderPaymentId == providerPaymentId, ct);

        public async Task<bool> WebhookSeenAsync(int paymentId, string eventId, CancellationToken ct)
        {
            var p = await _db.Payments.Where(x => x.Id == paymentId).Select(x => x.LastWebhookEventId).FirstOrDefaultAsync(ct);
            return p == eventId;
        }

        public async Task MarkWebhookSeenAsync(int paymentId, string eventId, CancellationToken ct)
        {
            var p = await _db.Payments.FirstAsync(x => x.Id == paymentId, ct);
            p.LastWebhookEventId = eventId;
            await _db.SaveChangesAsync(ct);
        }
    }
}
