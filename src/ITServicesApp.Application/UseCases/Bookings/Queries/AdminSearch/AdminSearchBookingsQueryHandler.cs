using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Interfaces;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Queries.AdminSearch
{
    public sealed class AdminSearchBookingsQueryHandler : IRequestHandler<AdminSearchBookingsQuery, IReadOnlyList<BookingResponseDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public AdminSearchBookingsQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow; _mapper = mapper;
        }

        public async Task<IReadOnlyList<BookingResponseDto>> Handle(AdminSearchBookingsQuery request, CancellationToken ct)
        {
            var all = await _uow.Bookings.ListAsync(b =>
                (request.FromUtc == null || b.ScheduledEndUtc >= request.FromUtc) &&
                (request.ToUtc == null || b.ScheduledStartUtc < request.ToUtc) &&
                (request.UserId == null || b.UserId == request.UserId) &&
                (request.TechnicianId == null || b.TechnicianId == request.TechnicianId), ct);

            var page = all.OrderByDescending(b => b.ScheduledStartUtc)
                          .Skip(request.Skip)
                          .Take(request.Take)
                          .ToList();

            // enrich via repository GetById to include navs (User/Technician/Payment)
            var responses = new List<BookingResponseDto>(page.Count);
            foreach (var b in page)
            {
                var withIncludes = await _uow.Bookings.GetByIdAsync(b.Id, ct) ?? b;
                responses.Add(_mapper.Map<BookingResponseDto>(withIncludes));
            }
            return responses;
        }
    }
}
