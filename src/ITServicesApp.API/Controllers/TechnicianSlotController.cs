using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.UseCases.Bookings.Commands.CreateTechnicianSlot;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/technician-slots")]
    [Authorize]
    public sealed class TechnicianSlotController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ITechnicianSlotService _slots;

        public TechnicianSlotController(IMediator mediator, ITechnicianSlotService slots)
        {
            _mediator = mediator;
            _slots = slots;
        }

        [HttpPost]
        public async Task<ActionResult<TechnicianSlotDto>> Create([FromBody] CreateTechnicianSlotDto dto, CancellationToken ct)
        {
            var result = await _mediator.Send(new CreateTechnicianSlotCommand(dto), ct);
            return Ok(result);
        }

        [HttpDelete("{technicianId:int}")]
        public async Task<IActionResult> DeleteByStart(int technicianId, [FromQuery] DateTime startUtc, CancellationToken ct)
        {
            await _slots.DeleteByStartAsync(technicianId, startUtc, ct);
            return NoContent();
        }

        [HttpGet("{technicianId:int}/day")]
        public async Task<ActionResult<IReadOnlyList<TechnicianSlotDto>>> GetForDay(int technicianId, [FromQuery] DateTime dayUtc, CancellationToken ct)
            => Ok(await _slots.GetAvailableAsync(technicianId, dayUtc, ct));
    }
}
