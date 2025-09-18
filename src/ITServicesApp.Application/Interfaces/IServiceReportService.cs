using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface IServiceReportService
    {
        Task<ServiceReportDto> SubmitAsync(ServiceReportDto dto, CancellationToken ct);
        Task<ServiceReportDto?> GetByBookingAsync(int bookingId, CancellationToken ct);
    }
}
