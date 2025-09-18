using MediatR;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.BlockUnavailability
{
    public sealed record BlockTechnicianUnavailabilityCommand(CreateUnavailabilityDto Dto) : IRequest<TechnicianUnavailabilityDto>;
}
