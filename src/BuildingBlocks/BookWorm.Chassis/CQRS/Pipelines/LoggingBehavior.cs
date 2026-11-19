using System.Diagnostics;
using Mediator;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.CQRS.Pipelines;

internal sealed partial class LoggingBehavior<TMessage, TResponse>(
    ILogger<LoggingBehavior<TMessage, TResponse>> logger
) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private const string Behavior = nameof(LoggingBehavior<,>);
    private const int SlowRequestThresholdSeconds = 3;

    private static readonly string _messageName = typeof(TMessage).Name;
    private static readonly string _responseName = typeof(TResponse).Name;

    private static readonly (string Name, bool IsSensitive)[] _properties =
    [
        .. typeof(TMessage)
            .GetProperties()
            .Select(p => (p.Name, p.IsDefined(typeof(DataClassificationAttribute), true))),
    ];

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            LogHandlingRequest(logger, Behavior, _messageName, _responseName);

            foreach (var (propName, isSensitive) in _properties)
            {
                if (isSensitive)
                {
                    LogPropertyRedacted(logger, Behavior, propName);
                    continue;
                }

                LogPropertyCaptured(logger, Behavior, propName);
            }
        }

        var start = Stopwatch.GetTimestamp();

        var response = await next(message, cancellationToken);

        var timeTaken = Stopwatch.GetElapsedTime(start);

        if (timeTaken.TotalSeconds >= SlowRequestThresholdSeconds)
        {
            LogSlowRequest(logger, Behavior, _messageName, timeTaken.TotalSeconds);
        }
        else
        {
            LogRequestHandled(
                logger,
                Behavior,
                _messageName,
                _responseName, // Log type name only to prevent PII exposure via response DTOs
                timeTaken.TotalMilliseconds
            );
        }

        return response;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "[{Behavior}] Handle request={Request} and response={Response}"
    )]
    private static partial void LogHandlingRequest(
        ILogger logger,
        string behavior,
        string request,
        string response
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "[{Behavior}] Property {Property} : [REDACTED]"
    )]
    private static partial void LogPropertyRedacted(
        ILogger logger,
        string behavior,
        string property
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "[{Behavior}] Property {Property} captured"
    )]
    private static partial void LogPropertyCaptured(
        ILogger logger,
        string behavior,
        string property
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "[{Behavior}] The request {Request} took {TimeTaken} seconds."
    )]
    private static partial void LogSlowRequest(
        ILogger logger,
        string behavior,
        string request,
        double timeTaken
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "[{Behavior}] The request handled {RequestName} with {Response} in {ElapsedMilliseconds} ms"
    )]
    private static partial void LogRequestHandled(
        ILogger logger,
        string behavior,
        string requestName,
        string response,
        double elapsedMilliseconds
    );
}
