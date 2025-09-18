using ITServicesApp.Application.DTOs;
using MediatR;

namespace ITServicesApp.Application.UseCases.Admin.Queries.GetDashboardStats
{
    public sealed record GetDashboardStatsQuery() : IRequest<DashboardStatsDto>;
}
