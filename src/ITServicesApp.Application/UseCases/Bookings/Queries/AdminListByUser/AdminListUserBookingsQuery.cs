using System;
using System.Collections.Generic;
using ITServicesApp.Application.DTOs;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Queries.AdminListByUser
{
    public sealed record AdminListUserBookingsQuery(
        int UserId,
        DateTime? FromUtc,
        DateTime? ToUtc,
        int Take = 100,
        int Skip = 0
    ) : IRequest<IReadOnlyList<BookingResponseDto>>;
}
