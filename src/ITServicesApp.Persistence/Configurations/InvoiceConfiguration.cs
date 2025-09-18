using ITServicesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Infrastructure.Persistence.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> b)
        {
            b.ToTable("Invoices");
            b.HasKey(x => x.Id);
            b.Property(x => x.Number).HasMaxLength(32).IsRequired();
            b.HasIndex(x => x.Number).IsUnique();
            b.HasOne<Booking>().WithOne().HasForeignKey<Invoice>(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}