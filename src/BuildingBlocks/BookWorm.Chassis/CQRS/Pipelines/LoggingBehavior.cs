using System.Diagnostics;
using System.Reflection;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.CQRS.Pipelines;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        const string behavior = nameof(LoggingBehavior<,>);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "[{Behavior}] Handle request={Request} and response={Response}",
                behavior,
                typeof(TRequest).FullName,
                typeof(TResponse).FullName
            );

            var props = new List<PropertyInfo>(message.GetType().GetProperties());
            foreach (PropertyInfo prop in props)
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

        if (timeTaken.Seconds > 3)
        {
            logger.LogWarning(
                "[{Behavior}] The request {Request} took {TimeTaken} seconds.",
                behavior,
                typeof(TRequest).FullName,
                timeTaken.Seconds
            );
        }
        else
        {
            logger.LogInformation(
                "[{Behavior}] The request handled {RequestName} with {Response} in {ElapsedMilliseconds} ms",
                behavior,
                typeof(TRequest).Name,
                response,
                Stopwatch.GetElapsedTime(start).TotalMilliseconds
            );
        }

        return response;
    }
}
