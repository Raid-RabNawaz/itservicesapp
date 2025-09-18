using ITServicesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Infrastructure.Persistence.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> b)
        {
            b.ToTable("Addresses");
            b.HasKey(x => x.Id);
            b.Property(x => x.Label).HasMaxLength(50);
            b.Property(x => x.Line1).HasMaxLength(200).IsRequired();
            b.Property(x => x.Line2).HasMaxLength(200);
            b.Property(x => x.City).HasMaxLength(100).IsRequired();
            b.Property(x => x.State).HasMaxLength(100).IsRequired();
            b.Property(x => x.PostalCode).HasMaxLength(20).IsRequired();
            b.Property(x => x.Country).HasMaxLength(2).IsRequired();
            b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.UserId, x.IsDefault });
        }
    }
}