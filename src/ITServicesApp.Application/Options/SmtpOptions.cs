namespace ITServicesApp.Application.Options
{
    public sealed class SmtpOptions
    {
        public const string SectionName = "Smtp";
        public string Host { get; set; } = default!;
        public int Port { get; set; } = 587;
        public bool UseSsl { get; set; } = true;
        public string User { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string From { get; set; } = default!;
    }
}
