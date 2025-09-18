using ITServicesApp.Persistence.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITServicesApp.Persistence.Configurations
{
    public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> b)
        {
            b.ToTable("AuditLogs");
            b.HasKey(x => x.Id);
            b.Property(x => x.EntityName).IsRequired().HasMaxLength(128);
            b.Property(x => x.EntityId).HasMaxLength(128);
            b.Property(x => x.Action).IsRequired().HasMaxLength(32);
            b.Property(x => x.ChangedAtUtc).IsRequired();
            b.Property(x => x.ChangesJson).IsRequired();
            b.HasIndex(x => new { x.EntityName, x.ChangedAtUtc });
        }
    }
}
