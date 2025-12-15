using EShop.Domain.Orders;
using EShop.Application.Common;

namespace EShop.Application.Orders;

public record GetOrderByIdQuery(Guid OrderId) : IQuery<Result<OrderDto?>>;

public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, Result<OrderDto?>>
{
    private readonly IOrderRepository _orderRepo;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepo)
    {
        _orderRepo = orderRepo;
    }

    public async Task<Result<OrderDto?>> HandleAsync(GetOrderByIdQuery query, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(new OrderId(query.OrderId), ct);

        if (order == null)
            return Result<OrderDto?>.Failure("order not found");

        return Result<OrderDto?>.Success((OrderDto)order);
    }
}
