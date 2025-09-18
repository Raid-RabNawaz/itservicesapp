using System;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface ITechnicianDashboardService
    {
        Task<TechnicianDashboardDto> GetAsync(int technicianId, DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken ct = default);
    }
}
