using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.BlockUnavailability
{
    public sealed class BlockTechnicianUnavailabilityCommandHandler : IRequestHandler<BlockTechnicianUnavailabilityCommand, TechnicianUnavailabilityDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public BlockTechnicianUnavailabilityCommandHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow; _mapper = mapper;
        }

        public async Task<TechnicianUnavailabilityDto> Handle(BlockTechnicianUnavailabilityCommand request, CancellationToken ct)
        {
            var entity = _mapper.Map<ITServicesApp.Domain.Entities.TechnicianUnavailability>(request.Dto);
            await _uow.TechnicianUnavailabilities.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return _mapper.Map<TechnicianUnavailabilityDto>(entity);
        }
    }
}
