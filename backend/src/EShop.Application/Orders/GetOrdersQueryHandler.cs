using EShop.Domain.Orders;
using EShop.Domain.Products;
using EShop.Application.Common;

namespace EShop.Application.Orders;

public class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, Result<GetOrdersResponse>>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;

    public GetOrdersQueryHandler(IOrderRepository orderRepo, IProductRepository productRepo)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
    }

    public async Task<Result<GetOrdersResponse>> HandleAsync(GetOrdersQuery query, CancellationToken ct = default)
    {
        var orders = await _orderRepo.GetAllAsync(ct);

        var filteredOrders = orders;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (Enum.TryParse<OrderStatus>(query.Status, ignoreCase: true, out var status))
            {
                filteredOrders = [.. orders.Where(o => o.Status == status)];
            }
        }
        if (!string.IsNullOrWhiteSpace(query.TrackingNumber))
        {
            filteredOrders = filteredOrders
                .Where(o => o.TrackingNumber.Value.Contains(query.TrackingNumber, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var allProductIds = filteredOrders
            .SelectMany(o => o.Items.Select(i => i.ProductId))
            .Distinct()
            .ToList();

        var products = await _productRepo.GetByIdsAsync(allProductIds, ct);
        var productNames = products.ToDictionary(p => p.Id.Value, p => p.Name);

        var dtos = filteredOrders.Select(o => OrderDto.FromOrder(o, productNames)).ToList();

        // Calculate statistics from all orders
        var statistics = new OrderStatistics(
            TotalOrders: orders.Count,
            PendingOrders: orders.Count(o => o.Status == OrderStatus.Pending),
            ProcessingOrders: orders.Count(o => o.Status == OrderStatus.Processing),
            ShippedOrders: orders.Count(o => o.Status == OrderStatus.Shipped),
            DeliveredOrders: orders.Count(o => o.Status == OrderStatus.Delivered),
            CompletedOrders: orders.Count(o => o.Status == OrderStatus.Completed),
            CancelledOrders: orders.Count(o => o.Status == OrderStatus.Cancelled)
        );

        var response = new GetOrdersResponse(dtos, statistics);
        return Result<GetOrdersResponse>.Success(response);
    }
}
