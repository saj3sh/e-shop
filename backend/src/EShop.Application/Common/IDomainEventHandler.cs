namespace EShop.Application.Common;

/// <summary>
/// Base interface for domain event handlers
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : Domain.Common.IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
