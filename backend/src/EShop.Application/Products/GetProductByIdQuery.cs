using EShop.Application.Common;

namespace EShop.Application.Products;

public record GetProductByIdQuery(Guid Id) : IQuery<Result<ProductDto?>>;
