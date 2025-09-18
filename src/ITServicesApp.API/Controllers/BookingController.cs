using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.UseCases.Bookings.Commands.CancelBooking;
using ITServicesApp.Application.UseCases.Bookings.Commands.CompleteBooking;
using ITServicesApp.Application.UseCases.Bookings.Commands.CreateBooking;
using ITServicesApp.Application.UseCases.Bookings.Commands.CreateGuestBooking;
using ITServicesApp.Application.UseCases.Bookings.Commands.RescheduleBooking;
using ITServicesApp.Application.UseCases.Bookings.Commands.UpdateBookingNotes;
using ITServicesApp.Application.UseCases.Bookings.Queries.GetAvailableSlots;
using ITServicesApp.Application.UseCases.Bookings.Queries.GetBookingById;
using ITServicesApp.Application.UseCases.Bookings.Queries.ListMyBookings;
using ITServicesApp.Application.UseCases.Bookings.Queries.ListByTechnician;
using ITServicesApp.Application.UseCases.Bookings.Queries.TechnicianAgenda;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class BookingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingController(IMediator mediator) => _mediator = mediator;

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookingResponseDto>> GetById(int id, CancellationToken ct)
        {
            var dto = await _mediator.Send(new GetBookingByIdQuery(id), ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpGet("me")]
        public async Task<ActionResult<IReadOnlyList<BookingDto>>> MyBookings([FromQuery] int take = 20, [FromQuery] int skip = 0, CancellationToken ct = default)
            => Ok(await _mediator.Send(new ListMyBookingsQuery(take, skip), ct));

        [HttpPost]
        [AllowAnonymous] // allow pre-auth booking if your flow supports it; otherwise require [Authorize]
        public async Task<ActionResult<BookingResponseDto>> Create([FromBody] CreateBookingDto dto, CancellationToken ct)
        {
            var idempotencyKey = Request.Headers["Idempotency-Key"].ToString();
            var created = await _mediator.Send(new CreateBookingCommand(dto, string.IsNullOrWhiteSpace(idempotencyKey) ? dto.ClientRequestId : idempotencyKey), ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        [HttpPost("guest")]
        [AllowAnonymous]
        public async Task<ActionResult<GuestBookingResponseDto>> CreateGuest([FromBody] GuestBookingRequestDto dto, CancellationToken ct)
        {
            var result = await _mediator.Send(new CreateGuestBookingCommand(dto), ct);
            if (result.RequiresLogin)
            {
                return Conflict(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Booking!.Id }, result);
        }


        [HttpPost("{id:int}/cancel")]
        public async Task<ActionResult<BookingDto>> Cancel(int id, CancellationToken ct)
            => Ok(await _mediator.Send(new CancelBookingCommand(id), ct));

        [HttpPost("{id:int}/reschedule")]
        public async Task<ActionResult<BookingDto>> Reschedule(int id, [FromBody] RescheduleRequest body, CancellationToken ct)
            => Ok(await _mediator.Send(new RescheduleBookingCommand(id, body.NewStartUtc, body.NewEndUtc, body.NewTechnicianId), ct));

        [HttpPost("{id:int}/complete")]
        public async Task<ActionResult<BookingDto>> Complete(int id, [FromQuery] DateTime? actualEndUtc, CancellationToken ct)
            => Ok(await _mediator.Send(new CompleteBookingCommand(id, actualEndUtc), ct));

        [HttpPatch("{id:int}/notes")]
        public async Task<ActionResult<BookingDto>> UpdateNotes(int id, [FromBody] UpdateNotesRequest body, CancellationToken ct)
            => Ok(await _mediator.Send(new UpdateBookingNotesCommand(new UpdateBookingNotesDto { BookingId = id, Notes = body.Notes }), ct));

        // Availability
        [AllowAnonymous]
        [HttpGet("available-slots")]
        public async Task<ActionResult<IReadOnlyList<TechnicianSlotDto>>> GetAvailableSlots(
            [FromQuery] int serviceCategoryId,
            [FromQuery] int serviceIssueId,
            [FromQuery] string dayUtc,
            [FromQuery] int? durationMinutes,
            CancellationToken ct)
        {
            if (!DateTime.TryParse(dayUtc, out var parsedDate))
            {
                return BadRequest("Invalid date format");
            }
            return Ok(await _mediator.Send(new GetAvailableSlotsQuery(serviceCategoryId, serviceIssueId, parsedDate, durationMinutes), ct));
        }

        // Technician views
        [HttpGet("by-technician/{technicianId:int}")]
        public async Task<ActionResult<IReadOnlyList<BookingDto>>> ListByTechnician(int technicianId, [FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, [FromQuery] int take = 50, [FromQuery] int skip = 0, CancellationToken ct = default)
            => Ok(await _mediator.Send(new ListByTechnicianQuery(technicianId, fromUtc, toUtc, take, skip), ct));

        [HttpGet("technician/{technicianId:int}/agenda")]
        public async Task<ActionResult<IReadOnlyList<TechnicianSlotDto>>> TechnicianAgenda(int technicianId, [FromQuery] DateTime dayUtc, CancellationToken ct)
            => Ok(await _mediator.Send(new TechnicianAgendaQuery(technicianId, dayUtc), ct));
    }

    public sealed class RescheduleRequest
    {
        public DateTime NewStartUtc { get; set; }
        public DateTime NewEndUtc { get; set; }
        public int? NewTechnicianId { get; set; }
    }

    public sealed class UpdateNotesRequest
    {
        public string? Notes { get; set; }
    }
}


