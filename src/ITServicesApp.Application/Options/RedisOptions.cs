namespace ITServicesApp.Application.Options
{
    public sealed class RedisOptions
    {
        public const string SectionName = "Redis";
        public bool Enabled { get; set; } = true;
        public string? ConnectionString { get; set; } = default!;
        public int Database { get; set; } = 0;
        public string KeyPrefix { get; set; } = "itservice:";
    }
}
