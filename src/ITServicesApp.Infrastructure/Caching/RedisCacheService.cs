using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ITServicesApp.Infrastructure.Caching
{
    public sealed class RedisCacheService : ICacheService
    {
        private readonly IDatabase _db;
        private readonly string _prefix;
        private readonly ILogger<RedisCacheService> _log;

        public RedisCacheService(IConnectionMultiplexer mux,
                                 IOptions<RedisOptions> options,
                                 ILogger<RedisCacheService> log)
        {
            if (mux is null) throw new InvalidOperationException("Redis multiplexer not configured.");
            var opt = options.Value ?? new RedisOptions();
            _db = mux.GetDatabase(opt.Database);
            _prefix = opt.KeyPrefix ?? string.Empty;
            _log = log;
        }

        private string Key(string k) => string.IsNullOrEmpty(_prefix) ? k : $"{_prefix}{k}";

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            var value = await _db.StringGetAsync(Key(key));
            if (!value.HasValue) return default;
            return JsonSerializer.Deserialize<T>(value!);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(value);
            return _db.StringSetAsync(Key(key), json, ttl);
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
            => _db.KeyDeleteAsync(Key(key));
    }
}
