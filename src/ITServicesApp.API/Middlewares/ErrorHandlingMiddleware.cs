
namespace ITServicesApp.API.Middlewares
{
    public sealed class ErrorHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ErrorHandlingMiddleware> _log;
        public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> log) => _log = log;

        public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
        {
            try
            {
                await next(ctx);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unhandled exception. TraceId={TraceId}", ctx.TraceIdentifier);
                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(new
                {
                    traceId = ctx.TraceIdentifier,
                    message = "An unexpected error occurred."
                });
            }
        }
    }
}
