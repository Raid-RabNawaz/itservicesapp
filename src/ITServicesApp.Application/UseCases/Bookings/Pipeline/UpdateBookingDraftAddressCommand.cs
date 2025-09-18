using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Pipeline
{
    public sealed record UpdateBookingDraftAddressCommand(Guid DraftId, BookingPipelineAddressDto Address) : IRequest<BookingPipelineStateDto>;

    public sealed class UpdateBookingDraftAddressCommandHandler : IRequestHandler<UpdateBookingDraftAddressCommand, BookingPipelineStateDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public UpdateBookingDraftAddressCommandHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<BookingPipelineStateDto> Handle(UpdateBookingDraftAddressCommand request, CancellationToken ct)
        {
            var draft = await _uow.BookingDrafts.GetAsync(request.DraftId, ct)
                        ?? throw new InvalidOperationException("Booking draft not found.");

            if (draft.Status != BookingDraftStatus.Pending)
            {
                throw new InvalidOperationException("Only pending drafts can be updated.");
            }

            var dto = request.Address ?? throw new ArgumentNullException(nameof(request.Address));

            draft.AddressLine1 = dto.Line1.Trim();
            draft.AddressLine2 = string.IsNullOrWhiteSpace(dto.Line2) ? null : dto.Line2.Trim();
            draft.City = dto.City.Trim();
            draft.State = string.IsNullOrWhiteSpace(dto.State) ? null : dto.State.Trim();
            draft.PostalCode = dto.PostalCode.Trim();
            draft.Country = dto.Country.Trim();
            draft.Notes = dto.Notes;

            await _uow.BookingDrafts.UpdateAsync(draft, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<BookingPipelineStateDto>(draft);
        }
    }
}
