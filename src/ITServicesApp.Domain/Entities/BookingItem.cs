using ITServicesApp.Domain.Base;

namespace ITServicesApp.Domain.Entities
{
    public class BookingItem : AuditableEntity
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = default!;

        public int ServiceIssueId { get; set; }
        public ServiceIssue ServiceIssue { get; set; } = default!;

        // Snapshot fields to preserve catalog info at time of booking
        public string ServiceName { get; set; } = string.Empty;
        public string? ServiceDescription { get; set; }

        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal LineTotal { get; set; }

        public string? Notes { get; set; }
    }
}
