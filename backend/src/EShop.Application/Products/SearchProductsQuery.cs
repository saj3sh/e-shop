using EShop.Domain.Products;
using EShop.Application.Common;

namespace EShop.Application.Products;

public record SearchProductsQuery(string? SearchTerm, int Page, int PageSize) : IQuery<Result<SearchProductsResult>>;

public record SearchProductsResult(List<ProductDto> Items, int TotalCount, int Page, int PageSize);

public record ProductDto(Guid Id, string Name, decimal Price, string Sku, string ManufacturedFrom, string ShippedFrom)
{
    public static explicit operator ProductDto(Product p) => new(
        p.Id.Value,
        p.Name,
        p.Price.Amount,
        p.Sku.Value,
        p.ManufacturedFrom,
        p.ShippedFrom
    );
}

public class SearchProductsQueryHandler : IQueryHandler<SearchProductsQuery, Result<SearchProductsResult>>
{
    private readonly IProductRepository _productRepo;

    public SearchProductsQueryHandler(IProductRepository productRepo)
    {
        _productRepo = productRepo;
    }

    public async Task<Result<SearchProductsResult>> HandleAsync(SearchProductsQuery query, CancellationToken ct = default)
    {
        const int maxPageSize = 25;
        var pageSize = Math.Min(query.PageSize, maxPageSize);

        var (items, totalCount) = await _productRepo.SearchAsync(query.SearchTerm, query.Page, pageSize, ct);

        var dtos = items.Select(p => (ProductDto)p).ToList();

        return Result<SearchProductsResult>.Success(new SearchProductsResult(
            dtos,
            totalCount,
            query.Page,
            pageSize
        ));
    }
}
