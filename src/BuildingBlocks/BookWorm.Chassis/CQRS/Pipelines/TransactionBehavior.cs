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
        if (message.GetType().GetCustomAttribute<TransactionalAttribute>() is not { } attr)
        {
            return await next(message, cancellationToken);
        }

        if (dbContext.Database.CurrentTransaction is not null)
        {
            return await next(message, cancellationToken);
        }

        var messageName = message.GetType().Name;
        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async ct =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                attr.IsolationLevel,
                ct
            );

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
                var response = await next(message, ct);

                await transaction.CommitAsync(ct);

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
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "[{Behavior}] Transaction {TransactionId} failed for {Request}, rolling back",
                    nameof(TransactionBehavior<,>),
                    transaction.TransactionId,
                    messageName
                );

                await transaction.RollbackAsync(ct);

                throw;
            }
        }, cancellationToken);
    }
}
