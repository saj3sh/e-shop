namespace EShop.Domain.Common;

/// <summary>
/// marker interface for domain events
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
