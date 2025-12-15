namespace EShop.Application.Common;

/// <summary>
/// Service for invalidating related cache entries
/// </summary>
public interface ICacheInvalidator
{
    Task InvalidateProductAsync(Guid productId, CancellationToken ct = default);
    Task InvalidateAllProductsAsync(CancellationToken ct = default);
}

public class CacheInvalidator : ICacheInvalidator
{
    private readonly ICache _cache;

    public CacheInvalidator(ICache cache)
    {
        _cache = cache;
    }

    public async Task InvalidateProductAsync(Guid productId, CancellationToken ct = default)
    {
        await _cache.RemoveAsync(CacheKeys.ProductById(productId), ct);

        // clear all search results because product data changed
        await _cache.RemoveByPrefixAsync("products:search:", ct);
    }

    public async Task InvalidateAllProductsAsync(CancellationToken ct = default)
    {
        await _cache.RemoveByPrefixAsync("products:", ct);
    }
}
