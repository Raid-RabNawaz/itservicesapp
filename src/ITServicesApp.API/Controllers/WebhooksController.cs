using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class WebhooksController : ControllerBase
    {
        private readonly IPaymentService _payments;

        public WebhooksController(IPaymentService payments) => _payments = payments;

        // Stripe webhook endpoint
        [HttpPost("stripe")]
        [AllowAnonymous]
        public async Task<IActionResult> Stripe(CancellationToken ct)
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();
            var sig = Request.Headers["Stripe-Signature"].ToString();

            // IPaymentService should internally validate signature + process events idempotently.
            await _payments.HandleStripeWebhookAsync(payload, sig, ct);

            return Ok(); // Respond 200 so Stripe stops retrying
        }
    }
}
