using System;
using ITServicesApp.Domain.Base;

namespace ITServicesApp.Domain.Entities
{
    public class Notification : AuditableEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public bool IsRead { get; set; }
        public DateTime? ReadAtUtc { get; set; }

        public User User { get; set; } = default!;
    }
}
