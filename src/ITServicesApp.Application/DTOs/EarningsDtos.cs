using System;
using System.Collections.Generic;

namespace ITServicesApp.Application.DTOs
{
    public sealed class TechnicianEarningsSummaryDto
    {
        public int TechnicianId { get; set; }
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public decimal Gross { get; set; }
        public decimal CommissionFees { get; set; }
        public decimal Net { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public sealed class TechnicianPayoutDto
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "Pending"; // Pending, Paid
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? PaidAtUtc { get; set; }
    }
}
