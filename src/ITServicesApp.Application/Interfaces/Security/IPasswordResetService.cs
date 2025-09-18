using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Application.Interfaces.Security
{
    public interface IPasswordResetService
    {
        Task<string> GenerateAndStoreTokenAsync(User user, int ttlMinutes, CancellationToken ct);
        Task<bool> VerifyAndConsumeAsync(User user, string token, CancellationToken ct);
        string HashToken(string token);
    }
}
