using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.Interfaces.Security;
using ITServicesApp.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/auth/password-reset")]
    public sealed class PasswordResetController : ControllerBase
    {
        private readonly IPasswordResetService _reset;
        private readonly IEmailService _email;
        private readonly ApplicationDbContext _db;
        private readonly IUserService _users; // to set password post-verify
        public PasswordResetController(IPasswordResetService reset, IEmailService email, ApplicationDbContext db, IUserService users)
        { _reset = reset; _email = email; _db = db; _users = users; }

        [HttpPost("request")]
        [AllowAnonymous]
        public async Task<IActionResult> Request([FromBody] RequestPasswordResetDto dto, CancellationToken ct)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == dto.Email, ct);
            if (user == null) return Accepted(); // do not reveal existence
            var token = await _reset.GenerateAndStoreTokenAsync(user, ttlMinutes: 120, ct);
            await _email.SendAsync(dto.Email, "Password reset", $"Your reset token: {token}", ct);
            return Accepted();
        }

        [HttpPost("confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> Confirm([FromBody] ConfirmPasswordResetDto dto, CancellationToken ct)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, ct);
            if (user == null) return NoContent();
            var ok = await _reset.VerifyAndConsumeAsync(user, dto.Token, ct);
            if (!ok) return BadRequest("Invalid or expired token");
            // TODO: ensure IUserService exposes a reset path that doesn't require current password
            await _users.SetPasswordByResetAsync(user.Id, dto.NewPassword, ct); // <-- add this method to your IUserService implementation
            return NoContent();
        }
    }
}