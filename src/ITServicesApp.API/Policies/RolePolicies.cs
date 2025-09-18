using Microsoft.AspNetCore.Authorization;

namespace ITServicesApp.API.Policies;

public class RolePolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string TechnicianOnly = "TechnicianOnly";
    public const string CustomerOnly = "CustomerOnly";

    public static void RegisterPolicies(AuthorizationOptions options)
    {
        options.AddPolicy(AdminOnly, policy => policy.RequireRole("Admin"));
        options.AddPolicy(TechnicianOnly, policy => policy.RequireRole("Technician"));
        options.AddPolicy(CustomerOnly, policy => policy.RequireRole("Customer"));
    }
}
