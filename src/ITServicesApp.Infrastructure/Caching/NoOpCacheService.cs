using System;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.Interfaces;

namespace ITServicesApp.Infrastructure.Caching
{
    public sealed class NoOpCacheService : ICacheService
    {
        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) => Task.FromResult<T?>(default);
        public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default) => Task.CompletedTask;
        public Task RemoveAsync(string key, CancellationToken ct = default) => Task.CompletedTask;
    }
}
