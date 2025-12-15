using EShop.Application.Common;
using EShop.Domain.Orders;

namespace EShop.Application.Orders.EventHandlers;

/// <summary>
/// Example domain event handler for OrderCompleted event
/// </summary>
public class OrderCompletedEventHandler : IDomainEventHandler<OrderCompleted>
{
    public OrderCompletedEventHandler()
    {
    }

    public async Task HandleAsync(OrderCompleted domainEvent, CancellationToken cancellationToken = default)
    {
        // Example: Send shipment notification, update customer loyalty points, etc.
        // TODO: Implement actual business logic
        // - Send delivery confirmation
        // - Update customer loyalty points
        // - Archive order data
        // - Trigger review request

        await Task.CompletedTask;
    }
}
