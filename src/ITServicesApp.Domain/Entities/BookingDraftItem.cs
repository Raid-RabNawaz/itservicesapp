using System;
using ITServicesApp.Domain.Base;

namespace ITServicesApp.Domain.Entities
{
    public class BookingDraftItem : AuditableEntity
    {
        public int Id { get; set; }
        public Guid DraftId { get; set; }
        public int ServiceIssueId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal? UnitPrice { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Notes { get; set; }

        public BookingDraft Draft { get; set; } = default!;
        public ServiceIssue? ServiceIssue { get; set; }
    }
}
