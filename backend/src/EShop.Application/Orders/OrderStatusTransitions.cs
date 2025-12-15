using EShop.Domain.Orders;

namespace EShop.Application.Orders;

public static class OrderStatusTransitions
{
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> AllowedTransitions = new()
    {
        [OrderStatus.Pending] = [OrderStatus.Processing, OrderStatus.Cancelled],
        [OrderStatus.Processing] = [OrderStatus.Shipped, OrderStatus.Cancelled],
        [OrderStatus.Shipped] = [OrderStatus.Delivered, OrderStatus.Cancelled],
        [OrderStatus.Delivered] = [OrderStatus.Completed, OrderStatus.Cancelled],
        [OrderStatus.Completed] = [],
        [OrderStatus.Cancelled] = []
    };

    public static bool IsTransitionAllowed(OrderStatus from, OrderStatus to)
    {
        return AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    public static string GetTransitionError(OrderStatus from, OrderStatus to)
    {
        if (from == OrderStatus.Completed)
            return "Cannot change status of a completed order";

        if (from == OrderStatus.Cancelled)
            return "Cannot change status of a cancelled order";

        return $"Invalid status transition from {from} to {to}";
    }
}
