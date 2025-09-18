using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface IEarningsService
    {
        Task<TechnicianEarningsSummaryDto> GetSummaryAsync(int technicianId, DateTime fromUtc, DateTime toUtc, CancellationToken ct);
        Task<IReadOnlyList<TechnicianPayoutDto>> ListPayoutsAsync(int technicianId, int take, int skip, CancellationToken ct);
    }
}
