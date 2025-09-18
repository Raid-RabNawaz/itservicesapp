using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface IInvoiceService
    {
        Task<IReadOnlyList<InvoiceDto>> ListMyInvoicesAsync(int take, int skip, CancellationToken ct);
        Task<InvoiceDto?> GetByBookingAsync(int bookingId, CancellationToken ct);
        Task<byte[]> RenderPdfAsync(int invoiceId, CancellationToken ct);
    }
}
