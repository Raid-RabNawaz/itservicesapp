using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/bookings/{bookingId:int}/report")]
    [Authorize]
    public sealed class BookingReportsController : ControllerBase
    {
        private readonly IServiceReportService _reports;
        public BookingReportsController(IServiceReportService reports) => _reports = reports;

        [HttpPost]
        public async Task<ActionResult<ServiceReportDto>> Submit(int bookingId, [FromBody] ServiceReportDto dto, CancellationToken ct)
        {
            dto.BookingId = bookingId;
            var created = await _reports.SubmitAsync(dto, ct);
            return Ok(created);
        }

        [HttpGet]
        public async Task<ActionResult<ServiceReportDto>> Get(int bookingId, CancellationToken ct)
        {
            var res = await _reports.GetByBookingAsync(bookingId, ct);
            return res is null ? NotFound() : Ok(res);
        }
    }
}