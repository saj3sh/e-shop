using EShop.Application.Common;
using EShop.Domain.Orders;
using EShop.Domain.Common;

namespace EShop.Application.Orders.EventHandlers;

/// <summary>
/// domain event handler for OrderStatusChanged event
/// </summary>
public class OrderStatusChangedEventHandler : IDomainEventHandler<OrderStatusChanged>
{
    private readonly IActivityLogRepository _activityLogRepo;

    public OrderStatusChangedEventHandler(IActivityLogRepository activityLogRepo)
    {
        _activityLogRepo = activityLogRepo;
    }

    public async Task HandleAsync(OrderStatusChanged domainEvent, CancellationToken cancellationToken = default)
    {
        var activityLog = ActivityLog.Create(
            entityType: "Order",
            entityId: domainEvent.OrderId.Value.ToString(),
            action: "OrderStatusChanged",
            details: $"Order {domainEvent.OrderId.Value} status changed from {domainEvent.OldStatus} to {domainEvent.NewStatus}"
        );

        await _activityLogRepo.AddAsync(activityLog, cancellationToken);
    }
}
