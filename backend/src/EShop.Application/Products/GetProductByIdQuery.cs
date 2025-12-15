using EShop.Domain.Products;
using EShop.Application.Common;

namespace EShop.Application.Products;

public record GetProductByIdQuery(Guid Id) : IQuery<Result<ProductDto?>>;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, Result<ProductDto?>>
{
    private readonly IProductRepository _productRepo;

    public GetProductByIdQueryHandler(IProductRepository productRepo)
    {
        _productRepo = productRepo;
    }

    public async Task<Result<ProductDto?>> HandleAsync(GetProductByIdQuery query, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(new ProductId(query.Id), ct);

        if (product == null)
            return Result<ProductDto?>.Failure("product not found");

        return Result<ProductDto?>.Success((ProductDto)product);
    }
}
