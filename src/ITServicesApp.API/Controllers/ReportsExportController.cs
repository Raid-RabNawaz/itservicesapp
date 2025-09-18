using ITServicesApp.Application.UseCases.Admin.Queries.GetRevenueReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/admin/exports")]
    [Authorize]
    public sealed class ReportsExportController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ReportsExportController(IMediator mediator) => _mediator = mediator;

        [HttpGet("revenue.csv")]
        public async Task<IActionResult> RevenueCsv(DateTime fromUtc, DateTime toUtc, string interval = "Daily", CancellationToken ct = default)
        {
            var dto = await _mediator.Send(new GetRevenueReportQuery(fromUtc, toUtc, interval), ct);
            var sb = new StringBuilder();
            sb.AppendLine("period_start_utc,amount,currency");
            foreach (var b in dto.Buckets)
                sb.AppendLine($"{b.PeriodStartUtc:O},{b.Amount},{b.Currency}");
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "revenue.csv");
        }
    }
}
