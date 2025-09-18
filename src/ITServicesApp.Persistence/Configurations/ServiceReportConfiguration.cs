using ITServicesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Infrastructure.Persistence.Configurations
{
    public class ServiceReportConfiguration : IEntityTypeConfiguration<ServiceReport>
    {
        public void Configure(EntityTypeBuilder<ServiceReport> b)
        {
            b.ToTable("ServiceReports");
            b.HasKey(x => x.Id);
            b.HasOne<Booking>().WithOne().HasForeignKey<ServiceReport>(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.ProblemsDiagnosed).HasMaxLength(4000);
            b.Property(x => x.ActionsTaken).HasMaxLength(4000);
            b.Property(x => x.PartsUsedCsv).HasMaxLength(2000);
        }
    }
}