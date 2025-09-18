using MediatR;

namespace ITServicesApp.Application.Behaviors
{
    public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly Abstractions.ICurrentUserService _current;
        public AuthorizationBehavior(Abstractions.ICurrentUserService current) => _current = current;
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct) => await next();
    }
}
