using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using MediatR;

namespace ITServicesApp.Application.UseCases.Admin.Queries.GetDashboardStats
{
    public sealed class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
    {
        private readonly IAdminDashboardService _svc;

        public GetDashboardStatsQueryHandler(IAdminDashboardService svc) => _svc = svc;

        public Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken ct)
            => _svc.GetStatsAsync(ct);
    }
}
