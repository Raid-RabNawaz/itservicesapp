using AutoMapper;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.Interfaces.Security;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;
using ITServicesApp.Persistence;
using ITServicesApp.Application.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

namespace ITServicesApp.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _passwordHasher; 
        private readonly IEmailService _email;
        private readonly IJwtTokenService _jwt;
        private readonly ICurrentUserService _current;
        private readonly ApplicationDbContext _db;
        private readonly IPasswordResetService _passwordReset;
        private readonly FrontendOptions _frontendOptions;

        public UserService(
            IUnitOfWork uow,
            IMapper mapper,
            IPasswordHasher passwordHasher,
            IEmailService email,
            IJwtTokenService jwt,
            ICurrentUserService current,
            ApplicationDbContext db,
            IPasswordResetService passwordReset,
            IOptions<FrontendOptions> frontendOptions)
        {
            _uow = uow;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _email = email;
            _jwt = jwt;
            _current = current;
            _db = db;
            _passwordReset = passwordReset;
            _frontendOptions = frontendOptions.Value;
        }

        public async Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct = default)
        {
            if (await _uow.Users.EmailExistsAsync(dto.Email, ct))
                throw new InvalidOperationException("Email already registered.");

            var provisionalPassword = Guid.NewGuid().ToString("N");

            var user = new User
            {
                Email = dto.Email.Trim(),
                FullName = dto.FullName?.Trim() ?? string.Empty,
                Role = dto.Role,
                PasswordHash = _passwordHasher.Hash(provisionalPassword),
                MustChangePassword = true
            };

            await _uow.Users.AddAsync(user, ct);
            await _uow.SaveChangesAsync(ct);

            var token = await _passwordReset.GenerateAndStoreTokenAsync(user, ttlMinutes: 1440, ct);
            await SendFirstLoginEmailAsync(user, token, ct);

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
        {
            if (await _uow.Users.EmailExistsAsync(dto.Email, ct))
                throw new InvalidOperationException("Email already registered.");

            var user = new User
            {
                Email = dto.Email.Trim(),
                FullName = dto.FullName?.Trim() ?? string.Empty,
                Role = UserRole.Customer,
                PasswordHash = _passwordHasher.Hash(dto.Password),
                MustChangePassword = false,
                PasswordChangedAt = DateTime.UtcNow
            };

            await _uow.Users.AddAsync(user, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<UserDto>(user);
        }

        public Task<UserDto?> GetAsync(int id, CancellationToken ct = default)
            => MapOrNull(_uow.Users.GetByIdAsync(id, ct));

        public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            var u = await _uow.Users.GetByEmailAsync(email, ct);
            return u is null ? null : _mapper.Map<UserDto>(u);
        }

        public async Task UpdateAsync(int id, UpdateUserDto dto, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByIdAsync(id, ct) ?? throw new System.InvalidOperationException("User not found.");
            _mapper.Map(dto, user);
            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task ChangePasswordAsync(ChangePasswordDto dto, CancellationToken ct = default)
        {
            var me = await _uow.Users.GetByIdAsync(_current.UserIdInt, ct)
                     ?? throw new InvalidOperationException("User not found.");

            if (!_passwordHasher.Verify(dto.CurrentPassword, me.PasswordHash))
                throw new InvalidOperationException("Invalid current password.");

            me.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
            me.MustChangePassword = false;
            me.PasswordChangedAt = DateTime.UtcNow;

            await _uow.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Admin-triggered: regenerate a first-login token and notify the user.
        /// </summary>
        public async Task SendFirstLoginSetupAsync(int userId, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByIdAsync(userId, ct)
                       ?? throw new InvalidOperationException("User not found.");

            var provisionalPassword = Guid.NewGuid().ToString("N");
            user.PasswordHash = _passwordHasher.Hash(provisionalPassword);
            user.MustChangePassword = true;

            await _uow.SaveChangesAsync(ct);
            var token = await _passwordReset.GenerateAndStoreTokenAsync(user, ttlMinutes: 1440, ct);
            await SendFirstLoginEmailAsync(user, token, ct);
        }

        private async Task<UserDto?> MapOrNull(Task<User?> t)
        {
            var u = await t;
            return u is null ? null : _mapper.Map<UserDto>(u);
        }

        public async Task<AuthTokenResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByEmailAsync(dto.Email, ct)
                       ?? throw new InvalidOperationException("Invalid credentials.");

            if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))
                throw new InvalidOperationException("Invalid credentials.");

            var userDto = _mapper.Map<UserDto>(user);
            var token = _jwt.CreateToken(userDto);

            return new AuthTokenResponseDto
            {
                Token = token,
                MustChangePassword = user.MustChangePassword,
                User = userDto
            };
        }

        public async Task<UserDto> GetMeAsync(CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByIdAsync(_current.UserIdInt, ct) ?? throw new InvalidOperationException("User not found.");
            return _mapper.Map<UserDto>(user);
        }

        public async Task CompleteFirstLoginAsync(FirstLoginPasswordSetupDto dto, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByEmailAsync(dto.Email, ct)
                       ?? throw new InvalidOperationException("User not found.");

            var verified = await _passwordReset.VerifyAndConsumeAsync(user, dto.Token, ct);
            if (!verified)
                throw new InvalidOperationException("Invalid or expired first-login token.");

            user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
            user.MustChangePassword = false;
            user.PasswordChangedAt = DateTime.UtcNow;

            await _uow.SaveChangesAsync(ct);
        }

        private Task SendFirstLoginEmailAsync(User user, string token, CancellationToken ct)
        {
            var subject = "Welcome to IT Services - Set your password";
            var safeName = WebUtility.HtmlEncode(user.FullName);
            var baseUrl = string.IsNullOrWhiteSpace(_frontendOptions.BaseUrl)
                ? "https://app.localhost"
                : _frontendOptions.BaseUrl.TrimEnd('/');
            var pathSegment = string.IsNullOrWhiteSpace(_frontendOptions.FirstLoginPath)
                ? "/first-login"
                : _frontendOptions.FirstLoginPath.StartsWith('/') ? _frontendOptions.FirstLoginPath : $"/{_frontendOptions.FirstLoginPath}";
            var link = $"{baseUrl}{pathSegment}?email={System.Uri.EscapeDataString(user.Email)}&token={System.Uri.EscapeDataString(token)}";
            var encodedLink = WebUtility.HtmlEncode(link);
            var code = WebUtility.HtmlEncode(token);

            var body = $@"<p>Hi {safeName},</p>
<p>Your account has been created. Use the button below to <strong>Set your password</strong>.</p>
<p><a href=""{encodedLink}"">Set your password</a></p>
<p>If the button does not work, copy this link into your browser:<br/>{encodedLink}</p>
<p>Or enter this one-time code: <strong>{code}</strong></p>
<p>Thanks,<br/>IT Services Team</p>";

            return _email.SendAsync(user.Email, subject, body, ct);
        }

        public async Task<IReadOnlyList<UserDto>> SearchAsync(string? q, int take, int skip, CancellationToken ct = default)
        {
            var query = _db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(u =>
                    EF.Functions.Like(u.Email, $"%{q}%") ||
                    EF.Functions.Like(u.FullName, $"%{q}%"));
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAtUtc)
                .Skip(skip)
                .Take(take)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    Role = u.Role,
                    MustChangePassword = u.MustChangePassword,
                    CreatedAtUtc = u.CreatedAtUtc,
                    ModifiedAtUtc = u.ModifiedAtUtc
                })
                .ToListAsync(ct);

            return users;
        }

        public async Task DisableAsync(int id, CancellationToken ct = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct)
                ?? throw new KeyNotFoundException($"User {id} not found.");

            // Soft-disable = soft-delete; aligns with your SoftDeleteInterceptor & global filter
            user.IsDeleted = true;
            user.ModifiedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
        }

        public async Task EnableAsync(int id, CancellationToken ct = default)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id, ct)
                ?? throw new KeyNotFoundException($"User {id} not found.");

            user.IsDeleted = false;
            user.ModifiedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
        }

        public async Task VerifyTechnicianAsync(int userId, CancellationToken ct = default)
        {
            // Simple approach: ensure a Technician record exists & activate it.
            var tech = await _db.Technicians.FirstOrDefaultAsync(t => t.UserId == userId, ct);

            if (tech == null)
            {
                // Create a blank-ish record and mark active.
                // You can set defaults or infer category later in admin UI.
                tech = new Technician
                {
                    UserId = userId,
                    ServiceCategoryId = 0, // TODO: set a real category or require one before verifying
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };
                await _db.Technicians.AddAsync(tech, ct);
            }
            else
            {
                tech.IsActive = true;
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task SetPasswordByResetAsync(int userId, string newPassword, CancellationToken ct = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                ?? throw new KeyNotFoundException($"User {userId} not found.");

            user.PasswordHash = _passwordHasher.Hash(newPassword);
            user.MustChangePassword = false;
            user.ModifiedAtUtc = DateTime.UtcNow;
            user.PasswordChangedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
        }

    }
}







