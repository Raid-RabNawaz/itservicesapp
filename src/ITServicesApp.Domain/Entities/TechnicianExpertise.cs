using System;

namespace ITServicesApp.Domain.Entities
{
    public class TechnicianExpertise
    {
        public int TechnicianId { get; set; }
        public Technician Technician { get; set; } = default!;

        public int ServiceIssueId { get; set; }
        public ServiceIssue ServiceIssue { get; set; } = default!;

        public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsPrimary { get; set; }
    }
}
