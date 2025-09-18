using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;

namespace ITServicesApp.Infrastructure.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IUserService _users; // your existing IUserService
        public AdminUserService(IUserService users) => _users = users;
        public Task<IReadOnlyList<UserDto>> SearchAsync(string? q, int take, int skip, CancellationToken ct)
            => _users.SearchAsync(q, take, skip, ct); // if not available, implement query in persistence
        public Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct) => _users.CreateAsync(dto, ct);
        public Task UpdateAsync(int id, UpdateUserDto dto, CancellationToken ct) => _users.UpdateAsync(id, dto, ct);
        public Task DisableAsync(int id, CancellationToken ct) => _users.DisableAsync(id, ct);
        public Task EnableAsync(int id, CancellationToken ct) => _users.EnableAsync(id, ct);
        public Task VerifyTechnicianAsync(int userId, CancellationToken ct) => _users.VerifyTechnicianAsync(userId, ct);
    }
}