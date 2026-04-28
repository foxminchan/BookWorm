using System.Reflection;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.CQRS.Pipelines;

internal sealed class TransactionBehavior<TMessage, TResponse>(
    DbContext dbContext,
    ILogger<TransactionBehavior<TMessage, TResponse>> logger
) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (
            message.GetType().GetCustomAttribute<TransactionalAttribute>() is not { } attr
            || dbContext.Database.CurrentTransaction is not null
        )
        {
            return await next(message, cancellationToken).ConfigureAwait(false);
        }

        var messageName = message.GetType().Name;
        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy
            .ExecuteAsync(
                async ct =>
                {
                    await using var transaction = await dbContext
                        .Database.BeginTransactionAsync(attr.IsolationLevel, ct)
                        .ConfigureAwait(false);

                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation(
                            "[{Behavior}] Begin transaction {TransactionId} for {Request}",
                            nameof(TransactionBehavior<,>),
                            transaction.TransactionId,
                            messageName
                        );
                    }

                    try
                    {
                        var response = await next(message, ct).ConfigureAwait(false);

                        await transaction.CommitAsync(ct).ConfigureAwait(false);

                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            logger.LogInformation(
                                "[{Behavior}] Committed transaction {TransactionId} for {Request}",
                                nameof(TransactionBehavior<,>),
                                transaction.TransactionId,
                                messageName
                            );
                        }

                        return response;
                    }
                    catch
                    {
                        await transaction.RollbackAsync(ct).ConfigureAwait(false);
                        throw;
                    }
                },
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
