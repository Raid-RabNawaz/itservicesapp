using ITServicesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITServicesApp.Persistence.Configurations
{
    public class TechnicianConfiguration : IEntityTypeConfiguration<Technician>
    {
        public void Configure(EntityTypeBuilder<Technician> b)
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.HourlyRate).HasPrecision(18, 2);

            b.HasOne(x => x.User)
             .WithOne(u => u.TechnicianProfile)
             .HasForeignKey<Technician>(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.ServiceCategory)
             .WithMany(c => c.Technicians)
             .HasForeignKey(x => x.ServiceCategoryId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.UserId).IsUnique(); // one tech per user
        }
    }
}
