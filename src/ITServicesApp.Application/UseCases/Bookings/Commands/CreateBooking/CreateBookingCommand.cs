using ITServicesApp.Application.DTOs;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.CreateBooking
{
    public sealed record CreateBookingCommand(CreateBookingDto Dto, string? ClientRequestId) : IRequest<BookingResponseDto>;
}
