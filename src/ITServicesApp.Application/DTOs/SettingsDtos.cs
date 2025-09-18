namespace ITServicesApp.Application.DTOs
{
    public class PlatformSettingsDto
    {
        public decimal TechnicianCommissionRate { get; set; } // 0.2 = 20%
        public int CancellationPolicyHours { get; set; } // e.g., free cancel until X hours before
        public string Currency { get; set; } = "USD";
    }

    public sealed class UpdatePlatformSettingsDto : PlatformSettingsDto { }

    public sealed class NotificationTemplateDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = default!; // e.g., booking.confirmed
        public string Channel { get; set; } = "Email"; // Email/SMS/Push
        public string Subject { get; set; } = default!;
        public string Body { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}