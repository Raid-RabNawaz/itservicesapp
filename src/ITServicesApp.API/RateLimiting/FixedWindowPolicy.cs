using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace ITServicesApp.API.RateLimiting
{
    public static class FixedWindowPolicy
    {
        public const string Name = "fixed";
        public static void Configure(RateLimiterOptions options)
        {
            options.AddFixedWindowLimiter(policyName: Name, opts =>
            {
                opts.PermitLimit = 100;
                opts.Window = TimeSpan.FromMinutes(1);
                opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opts.QueueLimit = 50;
            });
        }
    }
}
