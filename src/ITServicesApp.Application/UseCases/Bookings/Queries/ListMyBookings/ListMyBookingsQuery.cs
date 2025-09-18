using MediatR;
using System.Collections.Generic;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.UseCases.Bookings.Queries.ListMyBookings
{
    public sealed record ListMyBookingsQuery(int Take = 20, int Skip = 0) : IRequest<IReadOnlyList<BookingDto>>;
}
