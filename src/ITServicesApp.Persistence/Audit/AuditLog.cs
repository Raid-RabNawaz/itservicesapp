namespace ITServicesApp.Persistence.Audit
{
    public sealed class AuditLog
    {
        public int Id { get; set; }
        public string EntityName { get; set; } = default!;
        public string? EntityId { get; set; }
        public string Action { get; set; } = default!;
        public DateTime ChangedAtUtc { get; set; }
        public string ChangesJson { get; set; } = default!;
    }
}
