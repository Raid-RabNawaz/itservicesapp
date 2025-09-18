using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Interfaces;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Queries.ListByTechnician
{
    public sealed class ListByTechnicianQueryHandler : IRequestHandler<ListByTechnicianQuery, IReadOnlyList<BookingDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ListByTechnicianQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow; _mapper = mapper;
        }

        public async Task<IReadOnlyList<BookingDto>> Handle(ListByTechnicianQuery request, CancellationToken ct)
        {
            var items = await _uow.Bookings.ListAsync(
                b => b.TechnicianId == request.TechnicianId &&
                     b.ScheduledStartUtc < request.ToUtc &&
                     request.FromUtc < b.ScheduledEndUtc, ct);

            var page = items.OrderBy(b => b.ScheduledStartUtc)
                            .Skip(request.Skip)
                            .Take(request.Take)
                            .Select(_mapper.Map<BookingDto>)
                            .ToList();

            return page;
        }
    }
}
