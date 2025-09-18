namespace ITServicesApp.Application.DTOs
{
    public class AuthTokenResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public bool MustChangePassword { get; set; }
        public UserDto User { get; set; } = default!;
    }
}
