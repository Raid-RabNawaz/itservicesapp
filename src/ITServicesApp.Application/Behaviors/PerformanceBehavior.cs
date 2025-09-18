using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ITServicesApp.Application.Behaviors
{
    public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
        public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger) => _logger = logger;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            var res = await next();
            sw.Stop();
            _logger.LogInformation("{Req} took {Ms} ms", typeof(TRequest).Name, sw.ElapsedMilliseconds);
            return res;
        }
    }
}
