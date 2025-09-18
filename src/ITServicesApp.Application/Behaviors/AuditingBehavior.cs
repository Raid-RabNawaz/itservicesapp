using MediatR;
using Microsoft.Extensions.Logging;

namespace ITServicesApp.Application.Behaviors
{
    public sealed class AuditingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<AuditingBehavior<TRequest, TResponse>> _logger;
        public AuditingBehavior(ILogger<AuditingBehavior<TRequest, TResponse>> logger) => _logger = logger;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            _logger.LogDebug("Audit start {Req}", typeof(TRequest).Name);
            var res = await next();
            _logger.LogDebug("Audit end {Req}", typeof(TRequest).Name);
            return res;
        }
    }
}
