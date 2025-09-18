using ITServicesApp.Domain.Base;
using ITServicesApp.Domain.Enums;

namespace ITServicesApp.Domain.Entities
{
    public class Payment : AuditableEntity
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = default!;

        public PaymentMethod Method { get; set; }
        public string Status { get; set; } = "Pending"; // Authorized/Succeeded/Failed/Refunded
        public decimal Amount { get; set; }
        public string? Currency { get; set; }

        public string? ProviderPaymentId { get; set; }  // Stripe PaymentIntent Id
        public string? ProviderChargeId { get; set; }
        public string? LastWebhookEventId { get; set; }
    }
}
