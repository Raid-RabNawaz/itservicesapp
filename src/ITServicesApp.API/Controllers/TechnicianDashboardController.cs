using System;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/technicians/{technicianId:int}/dashboard")]
    [Authorize]
    public sealed class TechnicianDashboardController : ControllerBase
    {
        private readonly ITechnicianDashboardService _dashboard;

        public TechnicianDashboardController(ITechnicianDashboardService dashboard)
        {
            _dashboard = dashboard;
        }

        [HttpGet]
        public async Task<ActionResult<TechnicianDashboardDto>> Get(int technicianId, DateTime? fromUtc, DateTime? toUtc, CancellationToken ct)
            => Ok(await _dashboard.GetAsync(technicianId, fromUtc, toUtc, ct));
    }
}
