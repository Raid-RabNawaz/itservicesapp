using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.UseCases.Bookings.Queries.GetAvailableSlots
{
    public sealed class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, IReadOnlyList<TechnicianSlotDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetAvailableSlotsQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow; _mapper = mapper;
        }

        public async Task<IReadOnlyList<TechnicianSlotDto>> Handle(GetAvailableSlotsQuery request, CancellationToken ct)
        {
            var day = request.DayUtc.Date;
            var next = day.AddDays(1);
            var requiredMinutes = request.DurationMinutes ?? 60;

            var techIds = await _uow.Technicians.QueryQualifiedTechnicianIdsAsync(request.ServiceCategoryId, request.ServiceIssueId, ct);
            if (techIds.Count == 0) return Array.Empty<TechnicianSlotDto>();

            var slots = new List<TechnicianSlotDto>();
            foreach (var techId in techIds)
            {
                var daySlots = await _uow.TechnicianSlots.GetAvailableAsync(techId, day, ct);

                var booked = await _uow.Bookings.ListAsync(b =>
                    b.TechnicianId == techId &&
                    b.Status != BookingStatus.Cancelled &&
                    b.ScheduledStartUtc < next && day < b.ScheduledEndUtc, ct);

                var unav = await _uow.TechnicianUnavailabilities.ListForTechnicianAsync(techId, day, next, ct);

                foreach (var s in daySlots.OrderBy(s => s.StartUtc))
                {
                    if ((s.EndUtc - s.StartUtc).TotalMinutes < requiredMinutes) continue;

                    var availableWindowEnd = s.EndUtc;
                    var overlapsBooking = booked.Any(b => b.ScheduledStartUtc < availableWindowEnd && s.StartUtc < b.ScheduledEndUtc);
                    if (overlapsBooking) continue;

                    var overlapsUnav = unav.Any(u => u.StartUtc < availableWindowEnd && s.StartUtc < u.EndUtc);
                    if (overlapsUnav) continue;

                    var dto = _mapper.Map<TechnicianSlotDto>(s);
                    dto.IsAvailable = true;
                    slots.Add(dto);
                }
            }

            return slots.OrderBy(s => s.StartUtc).ToList();
        }
    }
}
