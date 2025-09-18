using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDto> CreateCashAsync(CreatePaymentDto dto, CancellationToken ct = default); // status -> Succeeded when tech collects cash
        Task<PaymentDto> CreateOnlineAsync(CreatePaymentDto dto, CancellationToken ct = default); // returns provider intent id
        Task HandleStripeEventAsync(string eventId, string type, string providerPaymentId, string? chargeId, string status, decimal? amount, string? currency, CancellationToken ct);
        Task HandleStripeWebhookAsync(string payload, string signature, CancellationToken ct);
        Task<PaymentDto?> GetByBookingAsync(int bookingId, CancellationToken ct = default);


    }
}
