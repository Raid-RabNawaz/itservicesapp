using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.BlockUnavailability
{
    public sealed class UnblockTechnicianUnavailabilityCommandHandler : IRequestHandler<UnblockTechnicianUnavailabilityCommand, Unit>
    {
        private readonly IUnitOfWork _uow;

        public UnblockTechnicianUnavailabilityCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Unit> Handle(UnblockTechnicianUnavailabilityCommand request, CancellationToken ct)
        {
            var u = await _uow.TechnicianUnavailabilities.GetByIdAsync(request.UnavailabilityId, ct)
                    ?? throw new InvalidOperationException("Unavailability not found.");
            _uow.TechnicianUnavailabilities.Delete(u);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
