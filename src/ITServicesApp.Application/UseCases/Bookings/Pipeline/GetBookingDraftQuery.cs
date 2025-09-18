using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Interfaces;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Pipeline
{
    public sealed record GetBookingDraftQuery(Guid DraftId) : IRequest<BookingPipelineStateDto>;

    public sealed class GetBookingDraftQueryHandler : IRequestHandler<GetBookingDraftQuery, BookingPipelineStateDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetBookingDraftQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<BookingPipelineStateDto> Handle(GetBookingDraftQuery request, CancellationToken ct)
        {
            var draft = await _uow.BookingDrafts.GetAsync(request.DraftId, ct)
                        ?? throw new InvalidOperationException("Booking draft not found.");

            return _mapper.Map<BookingPipelineStateDto>(draft);
        }
    }
}
