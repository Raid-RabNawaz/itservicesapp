using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default);

        Task<RevenueReportDto> GetRevenueAsync(DateTime fromUtc, DateTime toUtc, string interval, CancellationToken ct = default);
        Task<IReadOnlyList<TechnicianUtilizationDto>> GetTechnicianUtilizationAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
    }
}
