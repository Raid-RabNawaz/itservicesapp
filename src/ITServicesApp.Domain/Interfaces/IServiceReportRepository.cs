using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface IServiceReportRepository : IRepository<ServiceReport>
    {
        Task<ServiceReport?> GetByBookingAsync(int bookingId, CancellationToken ct);
    }
}