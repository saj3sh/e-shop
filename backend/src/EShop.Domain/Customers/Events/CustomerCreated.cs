namespace EShop.Domain.Customers;

public record CustomerCreated(CustomerId CustomerId, Email Email, DateTime OccurredAt) : Common.IDomainEvent;
