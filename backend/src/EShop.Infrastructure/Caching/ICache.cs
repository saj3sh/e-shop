namespace EShop.Infrastructure.Caching;

/// <summary>
/// cache abstraction for flexibility
/// </summary>
public interface ICache
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) where T : class;
    Task RemoveAsync(string key, CancellationToken ct = default);
}
