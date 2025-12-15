namespace EShop.Application.Common;

/// <summary>
/// minimal mediator interface for commands and queries
/// </summary>
public interface ICommand<out TResult>
{
}

public interface IQuery<out TResult>
{
}

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default);
}

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default);
}
