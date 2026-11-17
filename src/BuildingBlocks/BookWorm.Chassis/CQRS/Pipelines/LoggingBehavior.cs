using System.Diagnostics;
using Mediator;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.CQRS.Pipelines;

internal sealed class LoggingBehavior<TMessage, TResponse>(
    ILogger<LoggingBehavior<TMessage, TResponse>> logger
) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        const string behavior = nameof(LoggingBehavior<,>);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "[{Behavior}] Handle request={Request} and response={Response}",
                behavior,
                message.GetType().Name,
                typeof(TResponse).Name
            );

            var props = message.GetType().GetProperties();
            foreach (var prop in props)
            {
                var isSensitive = prop.IsDefined(typeof(DataClassificationAttribute), true);

                if (isSensitive)
                {
                    logger.LogInformation(
                        "[{Behavior}] Property {Property} : [REDACTED]",
                        behavior,
                        prop.Name
                    );
                    continue;
                }

                logger.LogInformation(
                    "[{Behavior}] Property {Property} captured",
                    behavior,
                    prop.Name
                );
            }
        }

        var start = Stopwatch.GetTimestamp();

        var response = await next(message, cancellationToken);

        var timeTaken = Stopwatch.GetElapsedTime(start);

        const int threshold = 3;

        if (timeTaken.TotalSeconds >= threshold)
        {
            logger.LogWarning(
                "[{Behavior}] The request {Request} took {TimeTaken} seconds.",
                behavior,
                message.GetType().Name,
                timeTaken.TotalSeconds
            );
        }
        else
        {
            logger.LogInformation(
                "[{Behavior}] The request handled {RequestName} with {Response} in {ElapsedMilliseconds} ms",
                behavior,
                message.GetType().Name,
                typeof(TResponse).Name, // Log type name only to prevent PII exposure via response DTOs
                timeTaken.TotalMilliseconds
            );
        }

        return response;
    }
}
