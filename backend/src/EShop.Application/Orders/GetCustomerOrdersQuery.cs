using EShop.Application.Common;

namespace EShop.Application.Orders;

public record GetCustomerOrdersQuery(Guid CustomerId) : IQuery<Result<List<OrderDto>>>;
