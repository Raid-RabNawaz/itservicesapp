using System;
using System.Collections.Generic;
using ITServicesApp.Domain.Base;
using ITServicesApp.Domain.Enums;

namespace ITServicesApp.Domain.Entities
{
    public class User : AuditableEntity
    {
        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public UserRole Role { get; set; } = UserRole.Customer;

        // First-login enforcement
        public bool MustChangePassword { get; set; } = true;
        public DateTimeOffset? PasswordChangedAt { get; set; }
        public string? PasswordResetTokenHash { get; set; }
        public DateTimeOffset? PasswordResetTokenExpiresAt { get; set; }

        // Navigation
        public Technician? TechnicianProfile { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<TechnicianReview> Reviews { get; set; } = new List<TechnicianReview>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
