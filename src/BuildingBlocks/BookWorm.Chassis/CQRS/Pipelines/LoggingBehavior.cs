using System.Diagnostics;
using System.Reflection;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.CQRS.Pipelines;

public sealed class LoggingBehavior<TMessage, TResponse>(
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

            var props = new List<PropertyInfo>(message.GetType().GetProperties());
            foreach (var prop in props)
            {
                var propValue = prop.GetValue(message, null);
                logger.LogInformation(
                    "[{Behavior}] Property {Property} : {@Value}",
                    behavior,
                    prop.Name,
                    propValue
                );
            }
        }

        var start = Stopwatch.GetTimestamp();

        var response = await next(message, cancellationToken);

        var timeTaken = Stopwatch.GetElapsedTime(start);

        const int threshold = 3;

        if (timeTaken.Seconds >= threshold)
        {
            logger.LogWarning(
                "[{Behavior}] The request {Request} took {TimeTaken} seconds.",
                behavior,
                message.GetType().Name,
                timeTaken.Seconds
            );
        }
        else
        {
            logger.LogInformation(
                "[{Behavior}] The request handled {RequestName} with {Response} in {ElapsedMilliseconds} ms",
                behavior,
                message.GetType().Name,
                response,
                Stopwatch.GetElapsedTime(start).TotalMilliseconds
            );
        }

        return response;
    }
}
