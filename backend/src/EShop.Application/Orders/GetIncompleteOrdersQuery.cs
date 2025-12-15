using EShop.Domain.Orders;
using EShop.Application.Common;

namespace EShop.Application.Orders;

public record GetIncompleteOrdersQuery() : IQuery<Result<List<OrderDto>>>;

public class GetIncompleteOrdersQueryHandler : IQueryHandler<GetIncompleteOrdersQuery, Result<List<OrderDto>>>
{
    private readonly IOrderRepository _orderRepo;

    public GetIncompleteOrdersQueryHandler(IOrderRepository orderRepo)
    {
        _orderRepo = orderRepo;
    }

    public async Task<Result<List<OrderDto>>> HandleAsync(GetIncompleteOrdersQuery query, CancellationToken ct = default)
    {
        var orders = await _orderRepo.GetIncompleteOrdersAsync(ct);
        var dtos = orders.Select(o => (OrderDto)o).ToList();

        return Result<List<OrderDto>>.Success(dtos);
    }
}
