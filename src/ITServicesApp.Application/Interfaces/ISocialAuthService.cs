

namespace ITServicesApp.Application.Interfaces
{
    public interface ISocialAuthService
    {
        Task<string> LoginWithProviderAsync(string provider, string idToken, CancellationToken ct);
    }
}
