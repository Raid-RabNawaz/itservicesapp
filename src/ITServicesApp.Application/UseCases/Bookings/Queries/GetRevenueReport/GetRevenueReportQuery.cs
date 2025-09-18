using System;
using ITServicesApp.Application.DTOs;
using MediatR;

namespace ITServicesApp.Application.UseCases.Admin.Queries.GetRevenueReport
{
    public sealed record GetRevenueReportQuery(
        DateTime FromUtc,
        DateTime ToUtc,
        string Interval // "Daily" | "Weekly" | "Monthly"
    ) : IRequest<RevenueReportDto>;
}
