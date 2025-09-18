using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System;
using System.Security.Claims;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.Interfaces.Security;

namespace ITServicesApp.Infrastructure.Services
{
    public class SocialAuthService : ISocialAuthService
    {
        private readonly IUserService _users;
        private readonly IJwtTokenService _jwt;

        public SocialAuthService(IUserService users, IJwtTokenService jwt)
        {
            _users = users;
            _jwt = jwt;
        }

        public async Task<string> LoginWithProviderAsync(string provider, string idToken, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentException("Provider is required.", nameof(provider));
            if (string.IsNullOrWhiteSpace(idToken))
                throw new ArgumentException("Id token is required.", nameof(idToken));

            provider = provider.Trim().ToLowerInvariant();
            if (provider != "google" && provider != "facebook")
                throw new InvalidOperationException($"Unsupported social provider '{provider}'.");

            JwtSecurityToken parsed;
            try
            {
                parsed = new JwtSecurityTokenHandler().ReadJwtToken(idToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Invalid id token format.", ex);
            }

            if (parsed.ValidTo < DateTime.UtcNow)
                throw new InvalidOperationException("Id token is expired.");

            var email = parsed.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value;
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("Id token does not include an email claim.");

            var fullName = parsed.Claims.FirstOrDefault(c => c.Type == "name")?.Value ??
                           parsed.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value ??
                           email.Split('@')[0];

            var user = await _users.GetByEmailAsync(email, ct);
            if (user is null)
            {
                user = await _users.RegisterAsync(new RegisterDto
                {
                    Email = email,
                    FullName = fullName,
                    Password = Guid.NewGuid().ToString("N")
                }, ct);
            }

            return _jwt.CreateToken(user);
        }
    }
}
