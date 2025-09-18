namespace ITServicesApp.Domain.Entities
{
    public class TechnicianPayout
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? PaidAtUtc { get; set; }
    }
}