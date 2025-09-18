using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class TechnicianController : ControllerBase
    {
        private readonly ITechnicianService _techs;

        public TechnicianController(ITechnicianService techs) => _techs = techs;

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<TechnicianProfileDto>> Get(int id, CancellationToken ct)
            => Ok(await _techs.GetProfileByIdAsync(id, ct));

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<TechnicianProfileDto>> Me(CancellationToken ct)
            => Ok(await _techs.GetMyProfileAsync(ct));

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateTechnicianProfileDto dto, CancellationToken ct)
        {
            await _techs.UpdateMyProfileAsync(dto, ct);
            return NoContent();
        }

        [HttpGet("{id:int}/calendar")]
        [Authorize]
        public async Task<ActionResult<IReadOnlyList<TechnicianSlotDto>>> Calendar(int id, [FromQuery] DateTime dayUtc, CancellationToken ct)
            => Ok(await _techs.GetCalendarAsync(id, dayUtc.Date, dayUtc.Date.AddDays(1), ct));
    }
}
