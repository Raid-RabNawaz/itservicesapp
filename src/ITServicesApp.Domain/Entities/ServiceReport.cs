namespace ITServicesApp.Domain.Entities
{
    public class ServiceReport
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string? ProblemsDiagnosed { get; set; }
        public string? ActionsTaken { get; set; }
        public string? PartsUsedCsv { get; set; } // store list as CSV for simplicity
        public int? TimeSpentMinutes { get; set; }
        public DateTime SubmittedAtUtc { get; set; }
    }
}