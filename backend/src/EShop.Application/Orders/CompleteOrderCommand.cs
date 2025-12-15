using EShop.Domain.Orders;
using EShop.Application.Common;

namespace EShop.Application.Orders;

public record CompleteOrderCommand(Guid OrderId) : ICommand<Result>;

public class CompleteOrderCommandHandler : ICommandHandler<CompleteOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepo;

    public CompleteOrderCommandHandler(IOrderRepository orderRepo)
    {
        _orderRepo = orderRepo;
    }

    public async Task<Result> HandleAsync(CompleteOrderCommand command, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(new OrderId(command.OrderId), ct);

        if (order == null)
            return Result.Failure("order not found");

        order.MarkAsCompleted();
        await _orderRepo.UpdateAsync(order, ct);

        return Result.Success();
    }
}
