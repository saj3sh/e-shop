namespace EShop.Domain.Orders;

public record OrderPlaced(OrderId OrderId, Customers.CustomerId CustomerId, DateTime OccurredAt) : Common.IDomainEvent;

public record OrderStatusChanged(OrderId OrderId, OrderStatus OldStatus, OrderStatus NewStatus, DateTime OccurredAt) : Common.IDomainEvent;
