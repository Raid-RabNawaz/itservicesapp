using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct = default); // uses first-login token email
        Task<UserDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
        Task<UserDto?> GetAsync(int id, CancellationToken ct = default);
        Task<UserDto?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task UpdateAsync(int id, UpdateUserDto dto, CancellationToken ct = default);
        Task ChangePasswordAsync(ChangePasswordDto dto, CancellationToken ct = default);
        Task SendFirstLoginSetupAsync(int userId, CancellationToken ct = default);

        Task<AuthTokenResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
        Task<UserDto> GetMeAsync(CancellationToken ct = default);
        Task CompleteFirstLoginAsync(FirstLoginPasswordSetupDto dto, CancellationToken ct = default);

        // --- Added admin/ops methods ---
        Task<IReadOnlyList<UserDto>> SearchAsync(string? q, int take, int skip, CancellationToken ct = default);
        Task DisableAsync(int id, CancellationToken ct = default); // soft disable
        Task EnableAsync(int id, CancellationToken ct = default);
        Task VerifyTechnicianAsync(int userId, CancellationToken ct = default);

        // Used by password reset flow (no current password required)
        Task SetPasswordByResetAsync(int userId, string newPassword, CancellationToken ct = default);
    }
}
