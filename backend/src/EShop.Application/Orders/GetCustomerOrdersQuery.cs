using EShop.Domain.Orders;
using EShop.Domain.Customers;
using EShop.Domain.Products;
using EShop.Application.Common;

namespace EShop.Application.Orders;

public record GetCustomerOrdersQuery(Guid CustomerId) : IQuery<Result<List<OrderDto>>>;

public class GetCustomerOrdersQueryHandler : IQueryHandler<GetCustomerOrdersQuery, Result<List<OrderDto>>>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;

    public GetCustomerOrdersQueryHandler(IOrderRepository orderRepo, IProductRepository productRepo)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
    }

    public async Task<Result<List<OrderDto>>> HandleAsync(GetCustomerOrdersQuery query, CancellationToken ct = default)
    {
        var orders = await _orderRepo.GetByCustomerIdAsync(new CustomerId(query.CustomerId), ct);

        // Get all unique product IDs from all orders
        var allProductIds = orders
            .SelectMany(o => o.Items.Select(i => i.ProductId))
            .Distinct()
            .ToList();

        var products = await _productRepo.GetByIdsAsync(allProductIds, ct);
        var productNames = products.ToDictionary(p => p.Id.Value, p => p.Name);

        var dtos = orders.Select(o => OrderDto.FromOrder(o, productNames)).ToList();

        return Result<List<OrderDto>>.Success(dtos);
    }
}
