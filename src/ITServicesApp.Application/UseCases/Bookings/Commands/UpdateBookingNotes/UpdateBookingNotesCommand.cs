using MediatR;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.UpdateBookingNotes
{
    public sealed record UpdateBookingNotesCommand(UpdateBookingNotesDto Dto) : IRequest<BookingDto>;
}
