using ITServicesApp.Domain.Base;

namespace ITServicesApp.Domain.Entities
{
    public class TechnicianReview : AuditableEntity
    {
        public int Id { get; set; }

        // Required FKs
        public int BookingId { get; set; }
        public int TechnicianId { get; set; }
        public int UserId { get; set; }

        // Review fields
        public int Rating { get; set; }          // 1..5
        public string? Comment { get; set; }     // optional, limited in config

        // Navigations
        public Booking Booking { get; set; } = default!;
        public Technician Technician { get; set; } = default!;
        public User User { get; set; } = default!;
    }
}
