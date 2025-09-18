namespace ITServicesApp.Domain.Entities
{
    public class NotificationTemplate
    {
        public int Id { get; set; }
        public string Key { get; set; } = default!;
        public string Channel { get; set; } = "Email";
        public string Subject { get; set; } = default!;
        public string Body { get; set; } = default!;
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ModifiedAtUtc { get; set; }
    }
}