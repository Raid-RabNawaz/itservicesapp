using ITServicesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Infrastructure.Persistence.Configurations
{
    public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
    {
        public void Configure(EntityTypeBuilder<NotificationTemplate> b)
        {
            b.ToTable("NotificationTemplates");
            b.HasKey(x => x.Id);
            b.Property(x => x.Key).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Key).IsUnique();
            b.Property(x => x.Channel).HasMaxLength(20).IsRequired();
        }
    }
}