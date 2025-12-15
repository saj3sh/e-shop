using EShop.Application.Common;

namespace EShop.Application.Orders;

public record GetOrderByIdQuery(Guid OrderId) : IQuery<Result<OrderDto?>>;
