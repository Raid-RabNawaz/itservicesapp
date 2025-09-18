using System;
using System.Collections.Generic;
using ITServicesApp.Domain.Base;
using ITServicesApp.Domain.Enums;

namespace ITServicesApp.Domain.Entities
{
    public class BookingDraft : AuditableEntity
    {
        public Guid Id { get; set; }
        public int? UserId { get; set; }
        public string? GuestFullName { get; set; }
        public string? GuestEmail { get; set; }
        public string? GuestPhone { get; set; }
        public int? ServiceCategoryId { get; set; }
        public int? ServiceIssueId { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Notes { get; set; }
        public BookingDraftStatus Status { get; set; } = BookingDraftStatus.Pending;
        public DateTime? ExpiresAtUtc { get; set; }
        public int? TechnicianId { get; set; }
        public DateTime? SlotStartUtc { get; set; }
        public DateTime? SlotEndUtc { get; set; }
        public int? SlotId { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public PaymentMethod PreferredPaymentMethod { get; set; } = PaymentMethod.Cash;

        public ICollection<BookingDraftItem> Items { get; set; } = new List<BookingDraftItem>();
    }
}
