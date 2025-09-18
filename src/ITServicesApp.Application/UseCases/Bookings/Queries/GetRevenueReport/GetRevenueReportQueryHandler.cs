using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using MediatR;

namespace ITServicesApp.Application.UseCases.Admin.Queries.GetRevenueReport
{
    public sealed class GetRevenueReportQueryHandler : IRequestHandler<GetRevenueReportQuery, RevenueReportDto>
    {
        private readonly IAdminDashboardService _svc;
        public GetRevenueReportQueryHandler(IAdminDashboardService svc) => _svc = svc;

        public Task<RevenueReportDto> Handle(GetRevenueReportQuery request, CancellationToken ct)
            => _svc.GetRevenueAsync(request.FromUtc, request.ToUtc, request.Interval, ct);
    }
}
