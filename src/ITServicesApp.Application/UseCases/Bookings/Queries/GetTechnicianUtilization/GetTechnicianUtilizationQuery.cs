using System;
using System.Collections.Generic;
using ITServicesApp.Application.DTOs;
using MediatR;

namespace ITServicesApp.Application.UseCases.Admin.Queries.GetTechnicianUtilization
{
    public sealed record GetTechnicianUtilizationQuery(
        DateTime FromUtc,
        DateTime ToUtc
    ) : IRequest<IReadOnlyList<TechnicianUtilizationDto>>;
}
