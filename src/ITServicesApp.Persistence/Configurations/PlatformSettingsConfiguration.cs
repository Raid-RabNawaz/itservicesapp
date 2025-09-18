using ITServicesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Infrastructure.Persistence.Configurations
{
    public class PlatformSettingsConfiguration : IEntityTypeConfiguration<PlatformSettings>
    {
        public void Configure(EntityTypeBuilder<PlatformSettings> b)
        {
            b.ToTable("PlatformSettings");
            b.HasKey(x => x.Id);
        }
    }
}