using System;

namespace ITServicesApp.Application.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public bool IsRead { get; set; }
        public DateTime? ReadAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
