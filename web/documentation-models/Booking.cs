using System;
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

        // Scheduling
        public DateTime ScheduledStartUtc { get; set; }
        public DateTime ScheduledEndUtc { get; set; }
        public BookingStatus Status { get; set; }

        // Details
        public string? Address { get; set; }
        public string? Notes { get; set; }

        // Payment (optional 1-1)
        public Payment? Payment { get; set; }

        // NEW: background reminder job id (Hangfire or other)
        public string? ReminderJobId { get; set; }

        // NEW: idempotency key from client; unique per user
        public string? ClientRequestId { get; set; }
    }
}
