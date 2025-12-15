using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using EShop.Application.Common;
using EShop.Domain.Common;

namespace EShop.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation with domain event dispatching
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(AppDbContext context, IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect all domain events from aggregates
        var aggregates = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        // Clear events before saving (prevents re-dispatching)
        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        // Save changes to database
        var result = await _context.SaveChangesAsync(cancellationToken);

        // Dispatch events after successful save
        await _eventDispatcher.DispatchAsync(domainEvents, cancellationToken);

        return result;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            return;
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }
}
