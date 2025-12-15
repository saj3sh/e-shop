namespace EShop.Application.Common;

public interface ICache
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task<bool> TryGetAsync<T>(string key, out T? value, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) where T : class;
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
}
