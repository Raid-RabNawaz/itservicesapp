namespace ITServicesApp.Domain.Entities
{
    public class PlatformSettings
    {
        public int Id { get; set; } // singleton row (Id = 1)
        public decimal TechnicianCommissionRate { get; set; }
        public int CancellationPolicyHours { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime ModifiedAtUtc { get; set; }
    }
}