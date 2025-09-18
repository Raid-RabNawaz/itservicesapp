using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ITServicesApp.Domain.Enums;

namespace ITServicesApp.API.Policies.Handlers
{
    public sealed class MustBeAdminHandler : AuthorizationHandler<ITServicesApp.API.Policies.Requirements.MustBeAdminRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ITServicesApp.API.Policies.Requirements.MustBeAdminRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == UserRole.Admin.ToString()))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
