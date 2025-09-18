using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _payments;

        public PaymentsController(IPaymentService payments) => _payments = payments;

        [HttpPost("cash")]
        public async Task<ActionResult<PaymentDto>> CreateCash([FromBody] CreatePaymentDto dto, CancellationToken ct)
        {
            if (dto.Method != PaymentMethod.Cash) dto.Method = PaymentMethod.Cash;
            var res = await _payments.CreateCashAsync(dto, ct);
            return Ok(res);
        }

        [HttpPost("online")]
        public async Task<ActionResult<PaymentDto>> CreateOnline([FromBody] CreatePaymentDto dto, CancellationToken ct)
        {
            if (dto.Method == PaymentMethod.Cash) return BadRequest("Use /cash endpoint for cash payments.");
            var res = await _payments.CreateOnlineAsync(dto, ct);
            return Ok(res);
        }

        [HttpGet("{bookingId:int}")]
        public async Task<ActionResult<PaymentDto>> GetByBooking(int bookingId, CancellationToken ct)
        {
            var res = await _payments.GetByBookingAsync(bookingId, ct);
            return res is null ? NotFound() : Ok(res);
        }
    }
}
