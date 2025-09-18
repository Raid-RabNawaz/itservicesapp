using System;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.UseCases.Bookings.Pipeline;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/bookings/pipeline")]
    public sealed class BookingPipelineController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingPipelineController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<BookingPipelineStateDto>> Start([FromBody] BookingPipelineStartRequestDto dto, CancellationToken ct)
        {
            var state = await _mediator.Send(new StartBookingDraftCommand(dto), ct);
            return CreatedAtAction(nameof(Get), new { id = state.Id }, state);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<BookingPipelineStateDto>> Get(Guid id, CancellationToken ct)
        {
            var state = await _mediator.Send(new GetBookingDraftQuery(id), ct);
            return Ok(state);
        }

        [HttpPut("{id:guid}/address")]
        [AllowAnonymous]
        public async Task<ActionResult<BookingPipelineStateDto>> UpdateAddress(Guid id, [FromBody] BookingPipelineAddressDto dto, CancellationToken ct)
        {
            var state = await _mediator.Send(new UpdateBookingDraftAddressCommand(id, dto), ct);
            return Ok(state);
        }

        [HttpPut("{id:guid}/slot")]
        [AllowAnonymous]
        public async Task<ActionResult<BookingPipelineStateDto>> SelectSlot(Guid id, [FromBody] BookingPipelineSlotRequestDto dto, CancellationToken ct)
        {
            var state = await _mediator.Send(new SelectBookingDraftSlotCommand(id, dto), ct);
            return Ok(state);
        }

        [HttpPost("{id:guid}/confirm")]
        [AllowAnonymous]
        public async Task<ActionResult<BookingPipelineSubmissionResultDto>> Confirm(Guid id, [FromBody] BookingPipelineConfirmDto dto, CancellationToken ct)
        {
            var result = await _mediator.Send(new ConfirmBookingDraftCommand(id, dto), ct);
            if (result.RequiresLogin)
            {
                return Conflict(result);
            }

            return Ok(result);
        }
    }
}
