using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace ITServicesApp.API.Observability.HealthChecks
{
    public static class HealthCheckResponseWriter
    {
        public static readonly HealthCheckOptions Options = new()
        {
            ResponseWriter = async (ctx, rpt) =>
            {
                ctx.Response.ContentType = "application/json";
                var payload = new
                {
                    status = rpt.Status.ToString(),
                    results = rpt.Entries.Select(e => new { key = e.Key, status = e.Value.Status.ToString(), e.Value.Description })
                };
                await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        };
    }

    public static class HealthCheckTags
    {
        public const string Database = "db";
        public const string Cache = "cache";
    }
}
