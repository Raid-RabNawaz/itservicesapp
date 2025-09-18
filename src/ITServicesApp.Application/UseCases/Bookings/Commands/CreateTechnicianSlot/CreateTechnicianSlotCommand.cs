using MediatR;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.CreateTechnicianSlot
{
    public sealed record CreateTechnicianSlotCommand(CreateTechnicianSlotDto Dto) : IRequest<TechnicianSlotDto>;
}
