using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using EShop.Application.Common;

namespace EShop.Infrastructure.Caching;

/// <summary>
/// memory cache implementation with IMemoryCache
/// </summary>
public class MemoryCacheAdapter : ICache
{
    private readonly IMemoryCache _cache;

    // track keys so we can delete by prefix (IMemoryCache doesn't expose its keys)
    private readonly ConcurrentDictionary<string, byte> _keyTracker = new();

    public MemoryCacheAdapter(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        var result = _cache.Get<T>(key);
        return Task.FromResult(result);
    }

    public Task<bool> TryGetAsync<T>(string key, out T? value, CancellationToken ct = default) where T : class
    {
        var found = _cache.TryGetValue<T>(key, out value);
        return Task.FromResult(found);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) where T : class
    {
        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }
        else
        {
            options.SetSlidingExpiration(TimeSpan.FromMinutes(10));
        }

        // registering cleanup callback to remove from tracker when evicted
        options.RegisterPostEvictionCallback((k, v, r, s) =>
        {
            _keyTracker.TryRemove(k.ToString()!, out _);
        });

        _cache.Set(key, value, options);
        _keyTracker.TryAdd(key, 0);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.Remove(key);
        _keyTracker.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var keysToRemove = _keyTracker.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _keyTracker.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
