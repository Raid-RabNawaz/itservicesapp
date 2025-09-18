using System;
using System.Collections.Generic;
using ITServicesApp.Application.DTOs;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Queries.AdminSearch
{
    public sealed record AdminSearchBookingsQuery(
        DateTime? FromUtc,
        DateTime? ToUtc,
        int? UserId,
        int? TechnicianId,
        int Take = 100,
        int Skip = 0
    ) : IRequest<IReadOnlyList<BookingResponseDto>>;
}
