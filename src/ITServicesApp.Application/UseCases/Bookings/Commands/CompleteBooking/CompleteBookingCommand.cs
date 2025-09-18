using System;
using MediatR;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.CompleteBooking
{
    public sealed record CompleteBookingCommand(int BookingId, DateTime? ActualEndUtc) : IRequest<BookingDto>;
}
