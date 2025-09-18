using ITServicesApp.Domain.Enums;

namespace ITServicesApp.Application.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public PaymentMethod Method { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? ProviderPaymentId { get; set; }
        public string? ProviderChargeId { get; set; }
    }

    public class CreatePaymentDto
    {
        public int BookingId { get; set; }
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
    }
}
