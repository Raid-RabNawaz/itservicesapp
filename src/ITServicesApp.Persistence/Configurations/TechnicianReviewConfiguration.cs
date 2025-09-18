using ITServicesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITServicesApp.Persistence.Configurations
{
    public class TechnicianReviewConfiguration : IEntityTypeConfiguration<TechnicianReview>
    {
        public void Configure(EntityTypeBuilder<TechnicianReview> b)
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Rating).IsRequired();
            b.Property(x => x.Comment).HasMaxLength(2000);

            // Enforce valid rating range at DB level
            b.HasCheckConstraint("CK_TechnicianReview_Rating_Range", "[Rating] BETWEEN 1 AND 5");

            // Required relationships, Restrict on delete to protect history
            b.HasOne(x => x.Booking)
             .WithMany() // keep Booking clean; enforce uniqueness via index below
             .HasForeignKey(x => x.BookingId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Technician)
             .WithMany(t => t.Reviews)
             .HasForeignKey(x => x.TechnicianId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.User)
             .WithMany(u => u.Reviews)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            // One review per booking
            b.HasIndex(x => x.BookingId).IsUnique();

            // Useful query/index patterns
            b.HasIndex(x => new { x.TechnicianId, x.UserId });
            b.HasIndex(x => new { x.TechnicianId, x.Rating });
        }
    }
}
