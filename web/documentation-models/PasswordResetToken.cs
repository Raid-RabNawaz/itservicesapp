namespace ITServicesApp.Domain.Entities
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UsedAtUtc { get; set; }
        public bool IsUsed => UsedAtUtc.HasValue;
    }
}