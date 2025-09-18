namespace ITServicesApp.Application.DTOs
{
    public sealed class DashboardStatsDto
    {
        public int TotalBookings { get; set; }
        public int UpcomingBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int ActiveTechnicians { get; set; }
        public int ActiveCustomers { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TechnicianNetRevenue { get; set; }
    }
}
