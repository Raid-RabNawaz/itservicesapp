using System;
using System.Collections.Generic;
using ITServicesApp.Application.DTOs;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Queries.ListByTechnician
{
    public sealed record ListByTechnicianQuery(
        int TechnicianId,
        DateTime FromUtc,
        DateTime ToUtc,
        int Take = 50,
        int Skip = 0
    ) : IRequest<IReadOnlyList<BookingDto>>;
}
