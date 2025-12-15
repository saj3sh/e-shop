using EShop.Application.Common;
using EShop.Domain.Orders;

namespace EShop.Application.Orders;

public class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand, Result>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepo, IUnitOfWork unitOfWork)
    {
        _orderRepo = orderRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(UpdateOrderStatusCommand command, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(new OrderId(command.OrderId), ct);
        if (order is null)
            return Result.Failure("Order not found");

        if (!Enum.TryParse<OrderStatus>(command.Status, ignoreCase: true, out var newStatus))
            return Result.Failure($"Invalid order status: {command.Status}");

        if (!OrderStatusTransitions.IsTransitionAllowed(order.Status, newStatus))
            return Result.Failure(OrderStatusTransitions.GetTransitionError(order.Status, newStatus));

        // Use domain method to update status and raise event
        order.UpdateStatus(newStatus);

        _orderRepo.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
