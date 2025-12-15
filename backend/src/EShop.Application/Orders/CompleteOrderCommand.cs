using EShop.Domain.Orders;
using EShop.Application.Common;

namespace EShop.Application.Orders;

public record CompleteOrderCommand(Guid OrderId) : ICommand<Result>;

public class CompleteOrderCommandHandler : ICommandHandler<CompleteOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteOrderCommandHandler(IOrderRepository orderRepo, IUnitOfWork unitOfWork)
    {
        _orderRepo = orderRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(CompleteOrderCommand command, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(new OrderId(command.OrderId), ct);

        if (order == null)
            return Result.Failure("order not found");

        order.MarkAsCompleted();
        _orderRepo.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
