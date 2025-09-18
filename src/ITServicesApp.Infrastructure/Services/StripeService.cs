using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Infrastructure.Services
{
    public class StripeService
    {
        private readonly IPaymentRepository _payments;
        private readonly IUnitOfWork _uow;

        public StripeService(IPaymentRepository payments, IUnitOfWork uow)
        {
            _payments = payments;
            _uow = uow;
        }

        // Simplified: in real code, call Stripe SDK and return PaymentIntent Id
        public Task<string> CreatePaymentIntentAsync(decimal amount, string currency, CancellationToken ct)
        {
            // TODO: integrate Stripe SDK, pass idempotency key
            var fakeId = "pi_" + System.Guid.NewGuid().ToString("N");
            return Task.FromResult(fakeId);
        }

        public async Task<Payment?> ApplyStripeEventAsync(string eventId, string type, string providerPaymentId, string? chargeId, string status, decimal? amount, string? currency, CancellationToken ct)
        {
            var payment = await _payments.GetByProviderPaymentIdAsync(providerPaymentId, ct);
            if (payment is null) return null;

            if (await _payments.WebhookSeenAsync(payment.Id, eventId, ct)) return null;

            payment.ProviderChargeId = chargeId ?? payment.ProviderChargeId;
            payment.Status = status;
            if (amount.HasValue) payment.Amount = amount.Value;
            if (!string.IsNullOrWhiteSpace(currency)) payment.Currency = currency;

            await _uow.SaveChangesAsync(ct);
            await _payments.MarkWebhookSeenAsync(payment.Id, eventId, ct);
            return payment;
        }
    }
}
