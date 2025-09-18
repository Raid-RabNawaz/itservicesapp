using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using MediatR;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.UseCases.Bookings.Queries.AdminListByUser
{
    public sealed class AdminListUserBookingsQueryHandler : IRequestHandler<AdminListUserBookingsQuery, IReadOnlyList<BookingResponseDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public AdminListUserBookingsQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow; _mapper = mapper;
        }

        public async Task<IReadOnlyList<BookingResponseDto>> Handle(AdminListUserBookingsQuery request, CancellationToken ct)
        {
            var all = await _uow.Bookings.ListAsync(b =>
                b.UserId == request.UserId &&
                (request.FromUtc == null || b.ScheduledEndUtc >= request.FromUtc) &&
                (request.ToUtc == null || b.ScheduledStartUtc < request.ToUtc), ct);

            var page = all.OrderByDescending(b => b.ScheduledStartUtc)
                          .Skip(request.Skip)
                          .Take(request.Take)
                          .ToList();

            var result = new List<BookingResponseDto>(page.Count);
            foreach (var b in page)
            {
                var withNavs = await _uow.Bookings.GetByIdAsync(b.Id, ct) ?? b;
                result.Add(_mapper.Map<BookingResponseDto>(withNavs));
            }
            return result;
        }
    }
}
