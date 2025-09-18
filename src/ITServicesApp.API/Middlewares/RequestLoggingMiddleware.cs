using System.Diagnostics;

namespace ITServicesApp.API.Middlewares
{
    public sealed class RequestLoggingMiddleware : IMiddleware
    {
        private readonly ILogger<RequestLoggingMiddleware> _log;
        public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> log) => _log = log;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var sw = Stopwatch.StartNew();
            await next(context);
            sw.Stop();
            _log.LogInformation("HTTP {Method} {Path} => {Status} in {Elapsed:0.000} ms",
                context.Request.Method, context.Request.Path.Value, context.Response.StatusCode, sw.Elapsed.TotalMilliseconds);
        }
    }
}
