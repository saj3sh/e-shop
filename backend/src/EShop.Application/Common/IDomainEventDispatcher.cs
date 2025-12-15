namespace EShop.Application.Common;

/// <summary>
/// Domain event dispatcher for publishing events to handlers
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(Domain.Common.IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task DispatchAsync(IEnumerable<Domain.Common.IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
