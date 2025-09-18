using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Queries.TechnicianAgenda
{
    public sealed class TechnicianAgendaQueryHandler : IRequestHandler<TechnicianAgendaQuery, IReadOnlyList<TechnicianSlotDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public TechnicianAgendaQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow; _mapper = mapper;
        }

        public async Task<IReadOnlyList<TechnicianSlotDto>> Handle(TechnicianAgendaQuery request, CancellationToken ct)
        {
            var day = request.DayUtc.Date;
            var next = day.AddDays(1);

            var slots = await _uow.TechnicianSlots.GetAvailableAsync(request.TechnicianId, day, ct);
            var unav = await _uow.TechnicianUnavailabilities.ListForTechnicianAsync(request.TechnicianId, day, next, ct);
            var bookings = await _uow.Bookings.ListAsync(b =>
                b.TechnicianId == request.TechnicianId &&
                b.Status != BookingStatus.Cancelled &&
                b.ScheduledStartUtc < next && day < b.ScheduledEndUtc, ct);

            var result = new List<TechnicianSlotDto>();

            // Base availability
            foreach (var s in slots.OrderBy(s => s.StartUtc))
            {
                var dto = _mapper.Map<TechnicianSlotDto>(s);
                dto.IsAvailable = true;
                result.Add(dto);
            }

            // Busy blocks from unavailability
            result.AddRange(unav.Select(u => new TechnicianSlotDto
            {
                TechnicianId = request.TechnicianId,
                StartUtc = u.StartUtc,
                EndUtc = u.EndUtc,
                IsAvailable = false
            }));

            // Busy blocks from bookings
            result.AddRange(bookings.Select(b => new TechnicianSlotDto
            {
                TechnicianId = request.TechnicianId,
                StartUtc = b.ScheduledStartUtc,
                EndUtc = b.ScheduledEndUtc,
                IsAvailable = false
            }));

            return result.OrderBy(x => x.StartUtc).ToList();
        }
    }
}
