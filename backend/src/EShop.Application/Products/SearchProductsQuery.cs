using EShop.Application.Common;

namespace EShop.Application.Products;

public record SearchProductsQuery(string? SearchTerm, int Page, int PageSize) : IQuery<Result<SearchProductsResult>>;

public record SearchProductsResult(List<ProductDto> Items, int TotalCount, int Page, int PageSize);
