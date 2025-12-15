using Microsoft.Extensions.DependencyInjection;
using EShop.Application.Common;
using EShop.Domain.Common;

namespace EShop.Infrastructure.Services;

/// <summary>
/// Domain event dispatcher implementation using service provider
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync));
            if (handleMethod != null)
            {
                var task = (Task?)handleMethod.Invoke(handler, new object[] { domainEvent, cancellationToken });
                if (task != null)
                {
                    await task;
                }
            }
        }
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }
}
