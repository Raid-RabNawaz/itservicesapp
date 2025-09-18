namespace ITServicesApp.Application.Options
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        // Token metadata
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;

        /// <summary>
        /// A 256-bit secret encoded as Base64 is recommended (e.g., 32 random bytes -> Base64).
        /// </summary>
        public string Key { get; set; } = default!;

        /// <summary>
        /// Optional key id, added to JWT header for rotation.
        /// </summary>
        public string? KeyId { get; set; }

        public int ExpiryMinutes { get; set; } = 60;

        // Validation tweaks (optional)
        public bool ValidateIssuer { get; set; } = true;
        public bool ValidateAudience { get; set; } = true;
        public int ClockSkewSeconds { get; set; } = 30;
    }
}
