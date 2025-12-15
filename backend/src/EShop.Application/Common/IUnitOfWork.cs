namespace EShop.Application.Common;

/// <summary>
/// Unit of Work pattern for coordinating transactions and domain events
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes and dispatches domain events
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new transaction
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
