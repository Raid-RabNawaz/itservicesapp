using Microsoft.AspNetCore.Authorization;

namespace ITServicesApp.API.Policies.Requirements
{
    public sealed class MustBeAdminRequirement : IAuthorizationRequirement { }
}
