using EShop.Domain.Products;
using EShop.Application.Common;
using Microsoft.Extensions.Options;

namespace EShop.Application.Products;

public class SearchProductsQueryHandler : IQueryHandler<SearchProductsQuery, Result<SearchProductsResult>>
{
    private readonly IProductRepository _productRepo;
    private readonly ICache _cache;
    private readonly CacheSettings _cacheSettings;

    public SearchProductsQueryHandler(IProductRepository productRepo, ICache cache, IOptions<CacheSettings> cacheSettings)
    {
        _productRepo = productRepo;
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<Result<SearchProductsResult>> HandleAsync(SearchProductsQuery query, CancellationToken ct = default)
    {
        const int maxPageSize = 25;
        var pageSize = Math.Min(query.PageSize, maxPageSize);

        var cacheKey = CacheKeys.ProductSearch(query.SearchTerm, query.Page, pageSize);

        // try cache first
        if (await _cache.TryGetAsync<SearchProductsResult>(cacheKey, out var cached, ct))
            return Result<SearchProductsResult>.Success(cached!);

        var (items, totalCount) = await _productRepo.SearchAsync(query.SearchTerm, query.Page, pageSize, ct);

        var dtos = items.Select(p => (ProductDto)p).ToList();

        var result = new SearchProductsResult(
            dtos,
            totalCount,
            query.Page,
            pageSize
        );

        // cache results
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.SearchCacheMinutes), ct);

        return Result<SearchProductsResult>.Success(result);
    }
}
