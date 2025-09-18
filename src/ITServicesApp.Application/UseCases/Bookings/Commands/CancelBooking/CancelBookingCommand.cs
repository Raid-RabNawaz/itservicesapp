using MediatR;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.CancelBooking
{
    public sealed record CancelBookingCommand(int BookingId) : IRequest<BookingDto>;
}
