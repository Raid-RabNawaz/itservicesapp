using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Persistence.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext db) : base(db) { }

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
            => _db.Users.AsNoTracking().AnyAsync(u => u.Email == email, ct);

        public Task SetPasswordHashAsync(User user, string newHash, CancellationToken ct = default)
        {
            user.PasswordHash = newHash;
            _db.Users.Update(user);
            return Task.CompletedTask;
        }
    }
}
