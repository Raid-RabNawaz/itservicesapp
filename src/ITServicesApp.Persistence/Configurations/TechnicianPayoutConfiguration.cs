using ITServicesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Infrastructure.Persistence.Configurations
{
    public class TechnicianPayoutConfiguration : IEntityTypeConfiguration<TechnicianPayout>
    {
        public void Configure(EntityTypeBuilder<TechnicianPayout> b)
        {
            b.ToTable("TechnicianPayouts");
            b.HasKey(x => x.Id);
            b.HasOne<Technician>().WithMany().HasForeignKey(x => x.TechnicianId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TechnicianId, x.CreatedAtUtc });
        }
    }
}