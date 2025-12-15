using System.Collections.Concurrent;
using System.Text.Json;

namespace EShop.Infrastructure.Caching;

/// <summary>
/// in-process memory cache, no external dependencies
/// </summary>
public class InMemoryCache : ICache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt == null || entry.ExpiresAt > DateTime.UtcNow)
            {
                return Task.FromResult(JsonSerializer.Deserialize<T>(entry.Value));
            }

            _cache.TryRemove(key, out _);
        }

        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) where T : class
    {
        var json = JsonSerializer.Serialize(value);
        var expiresAt = expiration.HasValue ? (DateTime?)DateTime.UtcNow.Add(expiration.Value) : null;

        _cache[key] = new CacheEntry(json, expiresAt);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    private record CacheEntry(string Value, DateTime? ExpiresAt);
}
