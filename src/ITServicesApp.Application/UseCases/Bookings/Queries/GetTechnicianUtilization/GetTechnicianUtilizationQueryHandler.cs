using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using MediatR;

namespace ITServicesApp.Application.UseCases.Admin.Queries.GetTechnicianUtilization
{
    public sealed class GetTechnicianUtilizationQueryHandler : IRequestHandler<GetTechnicianUtilizationQuery, IReadOnlyList<TechnicianUtilizationDto>>
    {
        private readonly IAdminDashboardService _svc;
        public GetTechnicianUtilizationQueryHandler(IAdminDashboardService svc) => _svc = svc;

        public Task<IReadOnlyList<TechnicianUtilizationDto>> Handle(GetTechnicianUtilizationQuery request, CancellationToken ct)
            => _svc.GetTechnicianUtilizationAsync(request.FromUtc, request.ToUtc, ct);
    }
}
