using ITServicesApp.Application.DTOs;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.CreateGuestBooking
{
    public sealed record CreateGuestBookingCommand(GuestBookingRequestDto Dto) : IRequest<GuestBookingResponseDto>;
}
