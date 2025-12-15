namespace EShop.Domain.Orders;

public record OrderPlaced(OrderId OrderId, Customers.CustomerId CustomerId, DateTime OccurredAt) : Common.IDomainEvent;

public record OrderCompleted(OrderId OrderId, DateTime OccurredAt) : Common.IDomainEvent;
