using System.Security.Claims;
using System.Text;
using ITServicesApp.Application.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ITServicesApp.API.Configuration.Extensions
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddJwtAndSignalRAuth(this IServiceCollection services, IConfiguration config)
        {
            // Bind Jwt options
            services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));

            // Read current values for AddJwtBearer config (Options is still the source of truth)
            var jwt = config.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                      ?? throw new InvalidOperationException("Jwt section missing.");

            var keyBytes = GetKeyBytes(jwt.Key);
            var signingKey = new SymmetricSecurityKey(keyBytes)
            {
                KeyId = string.IsNullOrWhiteSpace(jwt.KeyId) ? null : jwt.KeyId
            };

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;   // set false only for local dev if needed
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = jwt.ValidateIssuer,
                        ValidateAudience = jwt.ValidateAudience,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(jwt.ClockSkewSeconds),

                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = signingKey,

                        RoleClaimType = ClaimTypes.Role,
                        NameClaimType = ClaimTypes.NameIdentifier
                    };

                    // Allow SignalR to pass token via query string during WS negotiation
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = ctx =>
                        {
                            var accessToken = ctx.Request.Query["access_token"];
                            var path = ctx.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/hubs/notifications"))
                            {
                                ctx.Token = accessToken!;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();
            services.AddSignalR();

            return services;
        }

        // Same derivation rule as in JwtTokenService to avoid mismatches
        private static byte[] GetKeyBytes(string key)
        {
            try
            {
                var raw = Convert.FromBase64String(key);
                if (raw.Length < 32) throw new InvalidOperationException("JWT key must be at least 256 bits (Base64).");
                return raw;
            }
            catch
            {
                var raw = Encoding.UTF8.GetBytes(key);
                if (raw.Length < 32) throw new InvalidOperationException("JWT key must be at least 32 bytes when using plain text.");
                return raw;
            }
        }
    }
}
