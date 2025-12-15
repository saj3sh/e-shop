using EShop.Domain.Orders;
using EShop.Domain.Products;
using EShop.Application.Common;

namespace EShop.Application.Orders;

public record GetOrderByIdQuery(Guid OrderId) : IQuery<Result<OrderDto?>>;

public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, Result<OrderDto?>>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepo, IProductRepository productRepo)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
    }

    public async Task<Result<OrderDto?>> HandleAsync(GetOrderByIdQuery query, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(new OrderId(query.OrderId), ct);

        if (order == null)
            return Result<OrderDto?>.Failure("order not found");

        // Fetch product names for all items
        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _productRepo.GetByIdsAsync(productIds, ct);
        var productNames = products.ToDictionary(p => p.Id.Value, p => p.Name);

        return Result<OrderDto?>.Success(OrderDto.FromOrder(order, productNames));
    }
}
