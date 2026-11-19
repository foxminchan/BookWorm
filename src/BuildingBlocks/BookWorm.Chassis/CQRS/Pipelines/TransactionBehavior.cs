using System.Reflection;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.CQRS.Pipelines;

internal sealed partial class TransactionBehavior<TMessage, TResponse>(
    DbContext dbContext,
    ILogger<TransactionBehavior<TMessage, TResponse>> logger
) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private const string Behavior = nameof(TransactionBehavior<,>);
    private static readonly string _messageName = typeof(TMessage).Name;

    private static readonly TransactionalAttribute? _transactionalAttr =
        typeof(TMessage).GetCustomAttribute<TransactionalAttribute>();

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (_transactionalAttr is not { } attr || dbContext.Database.CurrentTransaction is not null)
        {
            return await next(message, cancellationToken);
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            async ct =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    attr.IsolationLevel,
                    ct
                );

                LogTransactionBegin(logger, Behavior, transaction.TransactionId, _messageName);

                try
                {
                    var response = await next(message, ct);

                    await transaction.CommitAsync(ct);

                    LogTransactionCommitted(
                        logger,
                        Behavior,
                        transaction.TransactionId,
                        _messageName
                    );

                    return response;
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            },
            cancellationToken
        );
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "[{Behavior}] Begin transaction {TransactionId} for {Request}"
    )]
    private static partial void LogTransactionBegin(
        ILogger logger,
        string behavior,
        Guid transactionId,
        string request
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "[{Behavior}] Committed transaction {TransactionId} for {Request}"
    )]
    private static partial void LogTransactionCommitted(
        ILogger logger,
        string behavior,
        Guid transactionId,
        string request
    );
}
