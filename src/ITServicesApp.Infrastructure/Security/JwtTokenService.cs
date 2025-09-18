using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces.Security;
using ITServicesApp.Application.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ITServicesApp.Infrastructure.Security
{
    public sealed class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _opt;
        public JwtTokenService(IOptions<JwtOptions> opt) => _opt = opt.Value;

        public string CreateToken(UserDto user)
        {
            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                // RFC-friendly ids
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(now).ToString(), ClaimValueTypes.Integer64),

                // Keep these for ASP.NET Core policies/identity
                new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("role", user.Role.ToString())
            };

            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                claims.Add(new Claim(ClaimTypes.Name, user.FullName));
                claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.FullName));
            }

            var keyBytes = GetKeyBytes(_opt.Key);
            var key = new SymmetricSecurityKey(keyBytes)
            {
                KeyId = string.IsNullOrWhiteSpace(_opt.KeyId) ? null : _opt.KeyId
            };
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                notBefore: now.AddSeconds(-_opt.ClockSkewSeconds), // tolerate small skew
                expires: now.AddMinutes(_opt.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static byte[] GetKeyBytes(string key)
        {
            // Prefer Base64 secrets (recommended).
            // If not Base64, fall back to UTF8 bytes to avoid crashing.
            try
            {
                var raw = Convert.FromBase64String(key);
                // Enforce 256-bit minimum for HS256
                if (raw.Length < 32) throw new InvalidOperationException("JWT key must be at least 256 bits.");
                return raw;
            }
            catch
            {
                var raw = Encoding.UTF8.GetBytes(key);
                if (raw.Length < 32)
                    throw new InvalidOperationException("JWT key must be at least 32 bytes when using plain text.");
                return raw;
            }
        }
    }
}
