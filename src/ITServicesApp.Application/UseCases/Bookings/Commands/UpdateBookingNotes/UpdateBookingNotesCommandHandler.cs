using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Events;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.UpdateBookingNotes
{
    public sealed class UpdateBookingNotesCommandHandler : IRequestHandler<UpdateBookingNotesCommand, BookingDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public UpdateBookingNotesCommandHandler(IUnitOfWork uow, IMapper mapper, IMediator mediator)
        {
            _uow = uow; _mapper = mapper; _mediator = mediator;
        }

        public async Task<BookingDto> Handle(UpdateBookingNotesCommand request, CancellationToken ct)
        {
            var dto = request.Dto;
            var b = await _uow.Bookings.GetByIdAsync(dto.BookingId, ct)
                    ?? throw new InvalidOperationException("Booking not found.");

            b.Notes = dto.Notes;
            _uow.Bookings.Update(b);
            await _uow.SaveChangesAsync(ct);

            // ⬇️ ensure we match the ctor: (BookingId, UserId, TechnicianId, ScheduledStartUtc, Reason)
            await _mediator.Publish(new BookingUpdatedDomainEvent(b.Id, b.UserId, b.TechnicianId, b.ScheduledStartUtc, "NotesUpdated"), ct);

            return _mapper.Map<BookingDto>(b);
        }
    }
}
