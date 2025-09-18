using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.UseCases.Admin.Queries.GetDashboardStats;
using ITServicesApp.Application.UseCases.Admin.Queries.GetRevenueReport;
using ITServicesApp.Application.UseCases.Admin.Queries.GetTechnicianUtilization;
using ITServicesApp.Application.UseCases.Bookings.Queries.AdminListByUser;
using ITServicesApp.Application.UseCases.Bookings.Queries.AdminSearch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // add admin policy/role
    public sealed class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAdminUserService _adminUsers;

        public AdminController(IMediator mediator, IAdminUserService adminUsers)
        {
            _mediator = mediator;
            _adminUsers = adminUsers;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardStatsDto>> Dashboard(CancellationToken ct)
            => Ok(await _mediator.Send(new GetDashboardStatsQuery(), ct));

        [HttpGet("revenue")]
        public async Task<ActionResult<RevenueReportDto>> Revenue([FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, [FromQuery] string interval = "Daily", CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetRevenueReportQuery(fromUtc, toUtc, interval), ct));

        [HttpGet("utilization")]
        public async Task<ActionResult<IReadOnlyList<TechnicianUtilizationDto>>> Utilization([FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, CancellationToken ct)
            => Ok(await _mediator.Send(new GetTechnicianUtilizationQuery(fromUtc, toUtc), ct));

        [HttpGet("bookings/search")]
        public async Task<ActionResult<IReadOnlyList<BookingResponseDto>>> SearchBookings([FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, [FromQuery] int? userId, [FromQuery] int? technicianId, [FromQuery] int take = 100, [FromQuery] int skip = 0, CancellationToken ct = default)
            => Ok(await _mediator.Send(new AdminSearchBookingsQuery(fromUtc, toUtc, userId, technicianId, take, skip), ct));

        [HttpGet("users/{userId:int}/bookings")]
        public async Task<ActionResult<IReadOnlyList<BookingResponseDto>>> ListUserBookings(int userId, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, [FromQuery] int take = 100, [FromQuery] int skip = 0, CancellationToken ct = default)
            => Ok(await _mediator.Send(new AdminListUserBookingsQuery(userId, fromUtc, toUtc, take, skip), ct));

        // USERS (ADMIN)
        [HttpGet("users")]
        public async Task<ActionResult<IReadOnlyList<UserDto>>> SearchUsers(
            [FromQuery] string? q, [FromQuery] int take = 50, [FromQuery] int skip = 0, CancellationToken ct = default)
            => Ok(await _adminUsers.SearchAsync(q, take, skip, ct));

        [HttpPost("users")]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto dto, CancellationToken ct)
            => Ok(await _adminUsers.CreateAsync(dto, ct));

        [HttpPut("users/{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto, CancellationToken ct)
        {
            await _adminUsers.UpdateAsync(id, dto, ct);
            return NoContent();
        }

        [HttpPost("users/{id:int}/disable")]
        public async Task<IActionResult> DisableUser(int id, CancellationToken ct)
        {
            await _adminUsers.DisableAsync(id, ct);
            return NoContent();
        }

        [HttpPost("users/{id:int}/enable")]
        public async Task<IActionResult> EnableUser(int id, CancellationToken ct)
        {
            await _adminUsers.EnableAsync(id, ct);
            return NoContent();
        }

        [HttpPost("users/{userId:int}/verify-technician")]
        public async Task<IActionResult> VerifyTechnician(int userId, CancellationToken ct)
        {
            await _adminUsers.VerifyTechnicianAsync(userId, ct);
            return NoContent();
        }
    }
}
