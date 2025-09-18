using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviews;
        private readonly ICurrentUserService _current;

        public ReviewsController(IReviewService reviews, ICurrentUserService current)
        {
            _reviews = reviews;
            _current = current;
        }

        [HttpPost]
        public async Task<ActionResult<TechnicianReviewDto>> Create([FromBody] CreateReviewDto dto, CancellationToken ct)
        {
            if (!_current.IsAuthenticated || _current.UserIdInt <= 0)
                return Forbid();

            var created = await _reviews.CreateAsync(_current.UserIdInt, dto, ct);
            return Ok(created);
        }

        [HttpGet("technician/{technicianId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<IReadOnlyList<TechnicianReviewDto>>> ListByTechnician(int technicianId, [FromQuery] int take = 20, [FromQuery] int skip = 0, CancellationToken ct = default)
            => Ok(await _reviews.ListByTechnicianAsync(technicianId, take, skip, ct));

        [HttpGet("technician/{technicianId:int}/avg")]
        [AllowAnonymous]
        public async Task<ActionResult<double>> AverageRating(int technicianId, CancellationToken ct)
            => Ok(await _reviews.GetAverageAsync(technicianId, ct));
    }
}
