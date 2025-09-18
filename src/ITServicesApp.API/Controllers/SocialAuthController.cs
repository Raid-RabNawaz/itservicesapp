using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/auth/social")]
    public sealed class SocialAuthController : ControllerBase
    {
        private readonly ISocialAuthService _social;
        public SocialAuthController(ISocialAuthService social) => _social = social;

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Login([FromBody] SocialLoginDto dto, CancellationToken ct)
        {
            var jwt = await _social.LoginWithProviderAsync(dto.Provider, dto.IdToken, ct);
            return string.IsNullOrWhiteSpace(jwt) ? Unauthorized() : Ok(jwt);
        }
    }
}
