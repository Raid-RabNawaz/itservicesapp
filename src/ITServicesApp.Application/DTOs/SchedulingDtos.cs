using System;

namespace ITServicesApp.Application.DTOs
{
    public class TechnicianSlotDto
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }

        // Legacy alias properties
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int? DurationMinutes { get; set; }
        public bool? IsAvailable { get; set; }
    }

    public class CreateTechnicianSlotDto
    {
        public int TechnicianId { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
    }

    public class TechnicianUnavailabilityDto
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public string? Reason { get; set; }
    }

    public class CreateUnavailabilityDto
    {
        public int TechnicianId { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public string? Reason { get; set; }
    }
}
