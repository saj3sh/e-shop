using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using EShop.Application.Common;
using EShop.Domain.Common;

namespace EShop.Infrastructure.Persistence;

/// <summary>
/// unit of work with domain event dispatching
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private IDbContextTransaction? _currentTransaction;
    private List<IDomainEvent> _pendingEvents = new();

    public UnitOfWork(AppDbContext context, IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();
        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();
        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        var result = await _context.SaveChangesAsync(cancellationToken);

        // if we're in a transaction, store events to dispatch after commit
        if (_currentTransaction != null)
        {
            _pendingEvents.AddRange(domainEvents);
        }
        else
        {
            await _eventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

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
            await _currentTransaction.CommitAsync(cancellationToken);

            // dispatch pending events after commit
            if (_pendingEvents.Any())
            {
                await _eventDispatcher.DispatchAsync(_pendingEvents, cancellationToken);
                _pendingEvents.Clear();
            }
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
            // Clear pending events on rollback - transaction failed, don't dispatch
            _pendingEvents.Clear();
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }
}
