using System;
using System.Collections.Generic;
using ITServicesApp.Domain.Enums;

namespace ITServicesApp.Application.DTOs
{
    public sealed class BookingSnapshotDto
    {
        public int BookingId { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public BookingStatus Status { get; set; }
        public string? ServiceName { get; set; }
        public string? CustomerName { get; set; }
        public string? TechnicianName { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public sealed class TechnicianDashboardDto
    {
        public int TechnicianId { get; set; }
        public TechnicianEarningsSummaryDto Earnings { get; set; } = new TechnicianEarningsSummaryDto();
        public int PendingConfirmationCount { get; set; }
        public int CompletedLast30Days { get; set; }
        public IReadOnlyList<BookingSnapshotDto> UpcomingBookings { get; set; } = Array.Empty<BookingSnapshotDto>();
        public IReadOnlyList<BookingSnapshotDto> RecentBookings { get; set; } = Array.Empty<BookingSnapshotDto>();
    }

    public sealed class CustomerDashboardDto
    {
        public int CustomerId { get; set; }
        public int ActiveRequests { get; set; }
        public decimal TotalSpentLast90Days { get; set; }
        public string Currency { get; set; } = "USD";
        public IReadOnlyList<BookingSnapshotDto> UpcomingBookings { get; set; } = Array.Empty<BookingSnapshotDto>();
        public IReadOnlyList<BookingSnapshotDto> RecentBookings { get; set; } = Array.Empty<BookingSnapshotDto>();
    }
}
