using Serilog.Context;

namespace ITServicesApp.API.Middlewares
{
    public sealed class CorrelationIdMiddleware : IMiddleware
    {
        private const string HeaderName = "X-Correlation-Id";

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Use the framework’s TraceIdentifier as correlation id (or read incoming header)
            var correlationId = context.TraceIdentifier;
            context.Response.Headers[HeaderName] = correlationId;

            var userName = context.User?.Identity?.IsAuthenticated == true
                ? context.User.Identity!.Name ?? "anonymous"
                : "anonymous";

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("User", userName))
            {
                await next(context);
            }
        }
    }
}
