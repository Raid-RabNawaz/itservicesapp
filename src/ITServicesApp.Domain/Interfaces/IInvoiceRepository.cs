using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<Invoice?> GetByBookingAsync(int bookingId, CancellationToken ct);
    }
}