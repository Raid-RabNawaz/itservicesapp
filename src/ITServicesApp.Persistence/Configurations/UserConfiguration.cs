using ITServicesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITServicesApp.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Email).IsRequired().HasMaxLength(256);
            b.Property(x => x.FullName).IsRequired().HasMaxLength(200);
            b.Property(x => x.PasswordHash).IsRequired();

            b.HasIndex(x => x.Email).IsUnique();

            b.HasOne(x => x.TechnicianProfile)
             .WithOne(t => t.User)
             .HasForeignKey<Technician>(t => t.UserId)
             .OnDelete(DeleteBehavior.Restrict); // keep tech/profile intact; prefer soft delete
        }
    }
}
