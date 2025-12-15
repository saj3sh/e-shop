using EShop.Application.Common;
using EShop.Domain.Orders;
using EShop.Domain.Common;

namespace EShop.Application.Orders.EventHandlers;

/// <summary>
/// Domain event handler for OrderPlaced event
/// </summary>
public class OrderPlacedEventHandler : IDomainEventHandler<OrderPlaced>
{
    private readonly IActivityLogRepository _activityLogRepo;

    public OrderPlacedEventHandler(IActivityLogRepository activityLogRepo)
    {
        _activityLogRepo = activityLogRepo;
    }

    public async Task HandleAsync(OrderPlaced domainEvent, CancellationToken cancellationToken = default)
    {
        // Log order placement activity
        var activityLog = ActivityLog.Create(
            entityType: "Order",
            entityId: domainEvent.OrderId.Value.ToString(),
            action: "OrderPlaced",
            userId: domainEvent.CustomerId.Value.ToString(),
            details: $"Order {domainEvent.OrderId.Value} was placed"
        );

        await _activityLogRepo.AddAsync(activityLog, cancellationToken);

        // TODO: Additional actions
        // - Send confirmation email
        // - Reserve inventory
        // - Notify other systems
    }
}
