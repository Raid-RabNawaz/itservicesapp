using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class MessagingController : ControllerBase
    {
        private readonly IMessageService _messages;
        public MessagingController(IMessageService messages) => _messages = messages;

        [HttpPost("threads/by-booking/{bookingId:int}")]
        public async Task<ActionResult<MessageThreadDto>> GetOrCreateForBooking(int bookingId, CancellationToken ct)
            => Ok(await _messages.GetOrCreateThreadForBookingAsync(bookingId, ct));

        [HttpGet("threads/{threadId:int}/messages")]
        public async Task<ActionResult<IReadOnlyList<MessageDto>>> List(int threadId, int take = 50, int skip = 0, CancellationToken ct = default)
            => Ok(await _messages.ListAsync(threadId, take, skip, ct));

        [HttpPost("messages")]
        public async Task<ActionResult<MessageDto>> Send([FromBody] SendMessageDto dto, CancellationToken ct)
            => Ok(await _messages.SendAsync(dto, ct));

        [HttpPost("threads/{threadId:int}/read")]
        public async Task<IActionResult> MarkThreadRead(int threadId, CancellationToken ct)
        {
            await _messages.MarkThreadReadAsync(threadId, ct);
            return NoContent();
        }
    }
}
