using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.CreateTechnicianSlot
{
    public sealed class CreateTechnicianSlotCommandHandler : IRequestHandler<CreateTechnicianSlotCommand, TechnicianSlotDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CreateTechnicianSlotCommandHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<TechnicianSlotDto> Handle(CreateTechnicianSlotCommand request, CancellationToken ct)
        {
            var dto = request.Dto;

            // Prevent overlap (validators also do this)
            var overlaps = await _uow.TechnicianSlots.ListAsync(s =>
                s.TechnicianId == dto.TechnicianId && s.StartUtc < dto.EndUtc && dto.StartUtc < s.EndUtc, ct);

            if (overlaps.Any())
                throw new InvalidOperationException("This slot overlaps with an existing slot.");

            var entity = _mapper.Map<ITServicesApp.Domain.Entities.TechnicianSlot>(dto);
            await _uow.TechnicianSlots.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<TechnicianSlotDto>(entity);
        }
    }
}
