using ITServicesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Infrastructure.Persistence.Configurations
{
    public class MessageThreadConfiguration : IEntityTypeConfiguration<MessageThread>
    {
        public void Configure(EntityTypeBuilder<MessageThread> b)
        {
            b.ToTable("MessageThreads");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.BookingId).IsUnique();
            b.HasMany(x => x.Messages).WithOne(x => x.Thread).HasForeignKey(x => x.ThreadId);
            b.HasOne<Booking>().WithOne().HasForeignKey<MessageThread>(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> b)
        {
            b.ToTable("Messages");
            b.HasKey(x => x.Id);
            b.Property(x => x.Body).HasMaxLength(4000).IsRequired();
            b.HasIndex(x => new { x.ThreadId, x.SentAtUtc });
        }
    }
}
