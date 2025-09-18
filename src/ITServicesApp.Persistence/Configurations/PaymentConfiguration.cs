using ITServicesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITServicesApp.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.Status).IsRequired().HasMaxLength(50);
            b.Property(x => x.ProviderPaymentId).HasMaxLength(200);
            b.Property(x => x.ProviderChargeId).HasMaxLength(200);
            b.Property(x => x.Currency).HasMaxLength(10);

            b.HasOne(x => x.Booking)
             .WithOne(bk => bk.Payment)
             .HasForeignKey<Payment>(x => x.BookingId)
             .OnDelete(DeleteBehavior.Restrict); // keep Payment data if Booking is manipulated

            b.HasIndex(x => x.ProviderPaymentId);
        }
    }
}
