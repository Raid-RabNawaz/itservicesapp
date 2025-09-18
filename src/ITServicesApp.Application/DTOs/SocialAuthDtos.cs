namespace ITServicesApp.Application.DTOs
{
    public sealed class SocialLoginDto
    {
        public string Provider { get; set; } = default!; // "google" | "facebook"
        public string IdToken { get; set; } = default!;  // Google ID token / Facebook token
    }
}