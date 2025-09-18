using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.BlockUnavailability
{
    public sealed record UnblockTechnicianUnavailabilityCommand(int UnavailabilityId) : IRequest<Unit>;
}
