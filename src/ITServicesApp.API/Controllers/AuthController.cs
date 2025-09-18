using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IUserService _users;

        public AuthController(IUserService users) => _users = users;

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto dto, CancellationToken ct)
        {
            var created = await _users.RegisterAsync(dto, ct);
            return CreatedAtAction(nameof(Profile), new { }, created);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthTokenResponseDto>> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            var response = await _users.LoginAsync(dto, ct);
            return Ok(response);
        }

        [HttpPost("first-login/complete")]
        [AllowAnonymous]
        public async Task<IActionResult> CompleteFirstLogin([FromBody] FirstLoginPasswordSetupDto dto, CancellationToken ct)
        {
            await _users.CompleteFirstLoginAsync(dto, ct);
            return NoContent();
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken ct)
        {
            await _users.ChangePasswordAsync(dto, ct);
            return NoContent();
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserDto>> Profile(CancellationToken ct)
        {
            var me = await _users.GetMeAsync(ct);
            return Ok(me);
        }
    }
}
