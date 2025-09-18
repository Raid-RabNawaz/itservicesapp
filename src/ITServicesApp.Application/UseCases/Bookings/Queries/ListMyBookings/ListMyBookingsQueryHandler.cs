using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.UseCases.Bookings.Queries.ListMyBookings
{
    public sealed class ListMyBookingsQueryHandler : IRequestHandler<ListMyBookingsQuery, IReadOnlyList<BookingDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _current;

        public ListMyBookingsQueryHandler(IUnitOfWork uow, IMapper mapper, ICurrentUserService current)
        {
            _uow = uow; _mapper = mapper; _current = current;
        }

        public async Task<IReadOnlyList<BookingDto>> Handle(ListMyBookingsQuery request, CancellationToken ct)
        {
            var all = await _uow.Bookings.ListForUserAsync(_current.UserIdInt, ct);
            var page = all.Skip(request.Skip).Take(request.Take).ToList();
            return page.Select(_mapper.Map<BookingDto>).ToList();
        }
    }
}
