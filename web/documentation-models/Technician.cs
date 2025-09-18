using System.Collections.Generic;
using ITServicesApp.Domain.Base;

namespace ITServicesApp.Domain.Entities
{
    public class Technician : AuditableEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }                  // 1:1 with User
        public int ServiceCategoryId { get; set; }       // primary category (simple model)
        public bool IsActive { get; set; } = true;

        // Profile
        public string? Bio { get; set; }
        public decimal? HourlyRate { get; set; }

        // Navigation
        public User User { get; set; } = default!;
        public ServiceCategory ServiceCategory { get; set; } = default!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<TechnicianSlot> Slots { get; set; } = new List<TechnicianSlot>();
        public ICollection<TechnicianUnavailability> Unavailabilities { get; set; } = new List<TechnicianUnavailability>();
        public ICollection<TechnicianReview> Reviews { get; set; } = new List<TechnicianReview>();
    }
}
