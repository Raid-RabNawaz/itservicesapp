using System.Security.Claims;
using ITServicesApp.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace ITServicesApp.Infrastructure.Security
{
    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _http;

        public CurrentUserService(IHttpContextAccessor http) => _http = http;

        public bool IsAuthenticated => _http.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public string? UserId => _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? _http.HttpContext?.User?.FindFirstValue("sub");

        public int UserIdInt
        {
            get
            {
                var s = UserId;
                return int.TryParse(s, out var id) ? id : 0;
            }
        }

        public string? Email => _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
        public string? Role => _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
    }
}
