namespace EShop.Application.Common;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(Domain.Common.IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task DispatchAsync(IEnumerable<Domain.Common.IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
