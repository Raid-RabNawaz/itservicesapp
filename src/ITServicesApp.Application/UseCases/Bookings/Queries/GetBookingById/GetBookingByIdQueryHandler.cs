using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.UseCases.Bookings.Queries.GetBookingById
{
    public sealed class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingResponseDto?>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetBookingByIdQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow; _mapper = mapper;
        }

        public async Task<BookingResponseDto?> Handle(GetBookingByIdQuery request, CancellationToken ct)
        {
            var b = await _uow.Bookings.GetByIdAsync(request.BookingId, ct);
            return b is null ? null : _mapper.Map<BookingResponseDto>(b);
        }
    }
}
