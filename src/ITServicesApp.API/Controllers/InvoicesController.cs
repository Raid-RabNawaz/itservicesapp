using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoices;
        public InvoicesController(IInvoiceService invoices) => _invoices = invoices;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<InvoiceDto>>> List(int take = 50, int skip = 0, CancellationToken ct = default)
            => Ok(await _invoices.ListMyInvoicesAsync(take, skip, ct));

        [HttpGet("by-booking/{bookingId:int}")]
        public async Task<ActionResult<InvoiceDto>> GetByBooking(int bookingId, CancellationToken ct)
        {
            var invoice = await _invoices.GetByBookingAsync(bookingId, ct);
            return invoice is null ? NotFound() : Ok(invoice);
        }

        [HttpGet("{invoiceId:int}/pdf")]
        public async Task<IActionResult> DownloadPdf(int invoiceId, CancellationToken ct)
        {
            var bytes = await _invoices.RenderPdfAsync(invoiceId, ct);
            return File(bytes, "application/pdf", $"invoice-{invoiceId}.pdf");
        }
    }
}
