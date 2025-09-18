using System;
using System.Collections.Generic;

namespace ITServicesApp.Application.DTOs
{
    public class RevenueBucketDto
    {
        public DateTime PeriodStartUtc { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public class RevenueReportDto
    {
        public decimal Total { get; set; }
        public string Currency { get; set; } = "USD";
        public List<RevenueBucketDto> Buckets { get; set; } = new();
    }

    public class TechnicianUtilizationDto
    {
        public int TechnicianId { get; set; }
        public string TechnicianName { get; set; } = "";
        public double AvailableHours { get; set; }
        public double BookedHours { get; set; }
        public double UtilizationPercent => AvailableHours > 0 ? (BookedHours / AvailableHours) * 100d : 0d;
    }
}
