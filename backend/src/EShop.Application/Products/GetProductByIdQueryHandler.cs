using EShop.Domain.Products;
using EShop.Application.Common;
using Microsoft.Extensions.Options;

namespace EShop.Application.Products;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, Result<ProductDto?>>
{
    private readonly IProductRepository _productRepo;
    private readonly ICache _cache;
    private readonly CacheSettings _cacheSettings;

    public GetProductByIdQueryHandler(IProductRepository productRepo, ICache cache, IOptions<CacheSettings> cacheSettings)
    {
        _productRepo = productRepo;
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<Result<ProductDto?>> HandleAsync(GetProductByIdQuery query, CancellationToken ct = default)
    {
        var cacheKey = CacheKeys.ProductById(query.Id);

        // try cache first
        if (await _cache.TryGetAsync<ProductDto>(cacheKey, out var cached, ct))
            return Result<ProductDto?>.Success(cached);

        var product = await _productRepo.GetByIdAsync(new ProductId(query.Id), ct);

        if (product == null)
            return Result<ProductDto?>.Failure("product not found");

        var dto = (ProductDto)product;

        // cache result
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(_cacheSettings.ProductCacheMinutes), ct);

        return Result<ProductDto?>.Success(dto);
    }
}
