using System;

namespace ITServicesApp.Application.DTOs
{
    public sealed class RequestPasswordResetDto
    {
        public string Email { get; set; } = default!;
    }

    public sealed class ConfirmPasswordResetDto
    {
        public string Email { get; set; } = default!;
        public string Token { get; set; } = default!; // one-time token sent via email
        public string NewPassword { get; set; } = default!;
    }
}
