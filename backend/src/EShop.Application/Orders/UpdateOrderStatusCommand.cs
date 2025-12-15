using EShop.Application.Common;

namespace EShop.Application.Orders;

public record UpdateOrderStatusCommand(Guid OrderId, string Status) : ICommand<Result>;
