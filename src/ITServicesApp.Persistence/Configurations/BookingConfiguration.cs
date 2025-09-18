using ITServicesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITServicesApp.Persistence.Configurations
{
    public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> b)
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Address).HasMaxLength(256);
            b.Property(x => x.AddressLine1).HasMaxLength(256);
            b.Property(x => x.AddressLine2).HasMaxLength(256);
            b.Property(x => x.City).HasMaxLength(128);
            b.Property(x => x.State).HasMaxLength(128);
            b.Property(x => x.PostalCode).HasMaxLength(32);
            b.Property(x => x.Country).HasMaxLength(128);
            b.Property(x => x.CustomerFullName).HasMaxLength(200);
            b.Property(x => x.CustomerEmail).HasMaxLength(256);
            b.Property(x => x.CustomerPhone).HasMaxLength(64);

            b.Property(x => x.Notes).HasMaxLength(2000);
            b.Property(x => x.ReminderJobId).HasMaxLength(200);

            b.Property(x => x.ClientRequestId).HasMaxLength(100);
            b.HasIndex(x => new { x.UserId, x.ClientRequestId }).IsUnique()
             .HasFilter("[ClientRequestId] IS NOT NULL");

            b.HasOne(x => x.User)
             .WithMany(u => u.Bookings)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.ClientNoAction);

            b.HasOne(x => x.Technician)
             .WithMany(t => t.Bookings)
             .HasForeignKey(x => x.TechnicianId)
             .OnDelete(DeleteBehavior.ClientNoAction);

            b.HasOne(x => x.Payment)
             .WithOne(p => p.Booking!)
             .HasForeignKey<Payment>(p => p.BookingId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
