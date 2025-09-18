using System;
using System.Collections.Generic;
using ITServicesApp.Domain.Base;
using ITServicesApp.Domain.Enums;

namespace ITServicesApp.Domain.Entities
{
    public class Booking : AuditableEntity
    {
        public int Id { get; set; }

        // Relations
        public int UserId { get; set; }
        public User User { get; set; } = default!;

        public int TechnicianId { get; set; }
        public Technician Technician { get; set; } = default!;

        public int ServiceCategoryId { get; set; }
        public ServiceCategory ServiceCategory { get; set; } = default!;

        public int ServiceIssueId { get; set; }
        public ServiceIssue ServiceIssue { get; set; } = default!;

        public ICollection<BookingItem> Items { get; set; } = new List<BookingItem>();

        // Scheduling
        public DateTime ScheduledStartUtc { get; set; }
        public DateTime ScheduledEndUtc { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime? TechnicianConfirmedAtUtc { get; set; }
        public DateTime? TechnicianEnRouteAtUtc { get; set; }
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }

        // Customer snapshot (for guest bookings / auditing)
        public string CustomerFullName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }

        // Address snapshot
        public int? AddressId { get; set; }
        public Address? AddressEntity { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }

        // Details
        public string? Address { get; set; }
        public string? Notes { get; set; }
        public bool RequiresManualConfirmation { get; set; }

        // Pricing snapshots
        public PaymentMethod PreferredPaymentMethod { get; set; } = PaymentMethod.Cash;
        public decimal EstimatedTotal { get; set; }
        public decimal? FinalTotal { get; set; }

        // Payment (optional 1-1)
        public Payment? Payment { get; set; }

        // NEW: background reminder job id (Hangfire or other)
        public string? ReminderJobId { get; set; }

        // NEW: idempotency key from client; unique per user
        public string? ClientRequestId { get; set; }
    }
}
