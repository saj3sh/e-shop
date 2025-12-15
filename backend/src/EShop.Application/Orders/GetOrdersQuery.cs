using EShop.Application.Common;

namespace EShop.Application.Orders;

public record GetOrdersQuery(string? Status = null, string? TrackingNumber = null) : IQuery<Result<GetOrdersResponse>>;

public record GetOrdersResponse(
    List<OrderDto> Orders,
    OrderStatistics Statistics
);

public record OrderStatistics(
    int TotalOrders,
    int PendingOrders,
    int ProcessingOrders,
    int ShippedOrders,
    int DeliveredOrders,
    int CompletedOrders,
    int CancelledOrders
);
