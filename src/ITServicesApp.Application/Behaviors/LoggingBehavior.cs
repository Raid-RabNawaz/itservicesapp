using MediatR;
using Microsoft.Extensions.Logging;

namespace ITServicesApp.Application.Behaviors
{
    public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            _logger.LogInformation("Handling {Request}", typeof(TRequest).Name);
            var response = await next();
            _logger.LogInformation("Handled {Request}", typeof(TRequest).Name);
            return response;
        }
    }
}
