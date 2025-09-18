using System;
using MediatR;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.RescheduleBooking
{
    public sealed record RescheduleBookingCommand(
        int BookingId,
        DateTime NewStartUtc,
        DateTime NewEndUtc,
        int? NewTechnicianId // null -> keep current
    ) : IRequest<BookingDto>;
}
