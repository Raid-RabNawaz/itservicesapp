using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface IPasswordResetRepository : IRepository<PasswordResetToken>
    {
        Task<PasswordResetToken?> FindValidAsync(int userId, string token, CancellationToken ct);
    }
}