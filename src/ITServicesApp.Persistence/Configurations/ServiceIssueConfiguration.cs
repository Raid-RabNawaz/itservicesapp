using ITServicesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITServicesApp.Persistence.Configurations
{
    public class ServiceIssueConfiguration : IEntityTypeConfiguration<ServiceIssue>
    {
        public void Configure(EntityTypeBuilder<ServiceIssue> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.EstimatedDurationMinutes);

            b.HasOne(x => x.ServiceCategory)
             .WithMany(c => c.ServiceIssues)
             .HasForeignKey(x => x.ServiceCategoryId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.ServiceCategoryId, x.Name }).IsUnique();
        }
    }
}
