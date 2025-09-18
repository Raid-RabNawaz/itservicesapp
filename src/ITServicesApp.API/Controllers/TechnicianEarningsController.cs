using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/technicians/{technicianId:int}/earnings")]
    [Authorize]
    public sealed class TechnicianEarningsController : ControllerBase
    {
        private readonly IEarningsService _earnings;
        public TechnicianEarningsController(IEarningsService earnings) => _earnings = earnings;

        [HttpGet("summary")]
        public async Task<ActionResult<TechnicianEarningsSummaryDto>> Summary(int technicianId, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
            => Ok(await _earnings.GetSummaryAsync(technicianId, fromUtc, toUtc, ct));

        [HttpGet("payouts")]
        public async Task<ActionResult<IReadOnlyList<TechnicianPayoutDto>>> Payouts(int technicianId, int take = 50, int skip = 0, CancellationToken ct = default)
            => Ok(await _earnings.ListPayoutsAsync(technicianId, take, skip, ct));
    }
}