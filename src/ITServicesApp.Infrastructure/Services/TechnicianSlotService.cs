using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Infrastructure.Services
{
    public class TechnicianSlotService : ITechnicianSlotService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public TechnicianSlotService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<TechnicianSlotDto> CreateAsync(CreateTechnicianSlotDto dto, CancellationToken ct = default)
        {
            var entity = _mapper.Map<Domain.Entities.TechnicianSlot>(dto);

            // Prevent overlap against existing slots
            var overlaps = await _uow.TechnicianSlots.ListAsync(s =>
                s.TechnicianId == dto.TechnicianId &&
                s.StartUtc < dto.EndUtc && dto.StartUtc < s.EndUtc, ct);

            if (overlaps.Any())
                throw new InvalidOperationException("This slot overlaps with an existing slot.");

            await _uow.TechnicianSlots.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return _mapper.Map<TechnicianSlotDto>(entity);
        }

        public async Task DeleteByStartAsync(int technicianId, DateTime startUtc, CancellationToken ct = default)
        {
            var existing = await _uow.TechnicianSlots.GetByTechAndStartAsync(technicianId, startUtc, ct)
                           ?? throw new InvalidOperationException("Slot not found.");
            _uow.TechnicianSlots.Delete(existing);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<TechnicianSlotDto>> ListDayAsync(int technicianId, DateTime dayUtc, CancellationToken ct = default)
        {
            var slots = await _uow.TechnicianSlots.GetAvailableAsync(technicianId, dayUtc, ct);
            return slots.Select(_mapper.Map<TechnicianSlotDto>).ToList();
        }

        public async Task<IReadOnlyList<TechnicianSlotDto>> GetAvailableAsync(int technicianId, DateTime dayUtc, CancellationToken ct = default)
        {
            var slots = await _uow.TechnicianSlots.GetAvailableAsync(technicianId, dayUtc.Date, ct);
            return slots.Select(_mapper.Map<TechnicianSlotDto>).ToList();
        }
    }
}
