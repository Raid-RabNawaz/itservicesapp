using System;

namespace ITServicesApp.Domain.Base
{
    public abstract class AuditableEntity
    {
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAtUtc { get; set; }
        public string? ModifiedBy { get; set; }

        // Soft-delete (enforced by interceptor + optional filters)
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        public string? DeletedBy { get; set; }
    }
}
