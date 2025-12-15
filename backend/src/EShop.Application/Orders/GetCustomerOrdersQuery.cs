using EShop.Domain.Orders;
using EShop.Domain.Customers;
using EShop.Application.Common;

namespace EShop.Application.Orders;

public record GetCustomerOrdersQuery(Guid CustomerId) : IQuery<Result<List<OrderDto>>>;

public class GetCustomerOrdersQueryHandler : IQueryHandler<GetCustomerOrdersQuery, Result<List<OrderDto>>>
{
    private readonly IOrderRepository _orderRepo;

    public GetCustomerOrdersQueryHandler(IOrderRepository orderRepo)
    {
        _orderRepo = orderRepo;
    }

    public async Task<Result<List<OrderDto>>> HandleAsync(GetCustomerOrdersQuery query, CancellationToken ct = default)
    {
        var orders = await _orderRepo.GetByCustomerIdAsync(new CustomerId(query.CustomerId), ct);
        var dtos = orders.Select(o => (OrderDto)o).ToList();

        return Result<List<OrderDto>>.Success(dtos);
    }
}
