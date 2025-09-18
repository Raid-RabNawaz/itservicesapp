using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<Payment?> GetByProviderPaymentIdAsync(string providerPaymentId, CancellationToken ct);
        Task<bool> WebhookSeenAsync(int paymentId, string eventId, CancellationToken ct);
        Task MarkWebhookSeenAsync(int paymentId, string eventId, CancellationToken ct);
    }
}
