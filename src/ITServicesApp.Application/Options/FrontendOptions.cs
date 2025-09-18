namespace ITServicesApp.Application.Options
{
    public class FrontendOptions
    {
        public const string SectionName = "Frontend";

        public string BaseUrl { get; set; } = "https://app.localhost";
        public string FirstLoginPath { get; set; } = "/first-login";
    }
}
