using System;
using ITServicesApp.Domain.Enums;

namespace ITServicesApp.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public UserRole Role { get; set; }
        public bool MustChangePassword { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ModifiedAtUtc { get; set; }
    }

    public class CreateUserDto
    {
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string? TempPassword { get; set; } = default!;
        public UserRole Role { get; set; } = UserRole.Customer;
        // Password is not accepted here; first-login token flow is used.
    }

    public class UpdateUserDto
    {
        public string FullName { get; set; } = default!;
        public UserRole Role { get; set; }
    }

    public class RegisterDto
    {
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class LoginDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }

    public class FirstLoginPasswordSetupDto
    {
        public string Email { get; set; } = default!;
        public string Token { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
