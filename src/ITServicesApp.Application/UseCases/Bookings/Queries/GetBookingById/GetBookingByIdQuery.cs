using MediatR;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.UseCases.Bookings.Queries.GetBookingById
{
    public sealed record GetBookingByIdQuery(int BookingId) : IRequest<BookingResponseDto?>;
}
