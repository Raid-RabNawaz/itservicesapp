using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface IAdminUserService
    {
        Task<IReadOnlyList<UserDto>> SearchAsync(string? q, int take, int skip, CancellationToken ct);
        Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct);
        Task UpdateAsync(int id, UpdateUserDto dto, CancellationToken ct);
        Task DisableAsync(int id, CancellationToken ct);
        Task EnableAsync(int id, CancellationToken ct);
        Task VerifyTechnicianAsync(int userId, CancellationToken ct);
    }
}
