using System;
using System.Collections.Generic;

namespace ITServicesApp.Application.DTOs
{
    public sealed class ServiceReportDto
    {
        public int BookingId { get; set; }
        public string? ProblemsDiagnosed { get; set; }
        public string? ActionsTaken { get; set; }
        public List<string>? PartsUsed { get; set; }
        public int? TimeSpentMinutes { get; set; }
        public DateTime SubmittedAtUtc { get; set; }
    }
}