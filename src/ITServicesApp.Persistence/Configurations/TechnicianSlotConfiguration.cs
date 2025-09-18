using ITServicesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITServicesApp.Persistence.Configurations
{
    public class TechnicianSlotConfiguration : IEntityTypeConfiguration<TechnicianSlot>
    {
        public void Configure(EntityTypeBuilder<TechnicianSlot> b)
        {
            b.HasKey(x => x.Id);

            b.HasOne(x => x.Technician)
             .WithMany(t => t.Slots)
             .HasForeignKey(x => x.TechnicianId)
             .OnDelete(DeleteBehavior.Cascade); // child of Technician

            b.HasIndex(x => new { x.TechnicianId, x.StartUtc, x.EndUtc });
        }
    }
}
