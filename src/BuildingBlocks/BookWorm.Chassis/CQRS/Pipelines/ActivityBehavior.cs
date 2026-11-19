using System.Reflection;
using BookWorm.Chassis.CQRS.Command;
using BookWorm.Chassis.CQRS.Query;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.CQRS.Pipelines;

internal sealed partial class ActivityBehavior<TMessage, TResponse>(
    IActivityScope activityScope,
    CommandHandlerMetrics commandMetrics,
    QueryHandlerMetrics queryMetrics,
    ILogger<ActivityBehavior<TMessage, TResponse>> logger
) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : notnull
{
    private const string Behavior = nameof(ActivityBehavior<,>);
    private static readonly string _messageType = typeof(TMessage).Name;
    private static readonly string _responseType = typeof(TResponse).Name;
    private static readonly string _handlerName = $"{_messageType}Handler";
    private static readonly string _activityName = $"{_handlerName}/{_messageType}";

    private static readonly bool _ignoreOTel =
        typeof(TMessage).GetCustomAttribute<IgnoreOTelOnHandlerAttribute>() is not null;

    private static readonly bool _isCommand = _messageType.EndsWith(
        nameof(Command),
        StringComparison.OrdinalIgnoreCase
    );

    private static readonly string _tagName = _isCommand
        ? TelemetryTags.Commands.Command
        : TelemetryTags.Queries.Query;

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        LogHandlingRequest(logger, Behavior, _messageType, _responseType);

        if (_ignoreOTel)
        {
            return await next(message, cancellationToken);
        }

        var startingTimestamp = _isCommand
            ? commandMetrics.CommandHandlingStart(_handlerName)
            : queryMetrics.QueryHandlingStart(_handlerName);

        try
        {
            return await activityScope.Run(
                _activityName,
                async (_, ct) => await next(message, ct),
                new() { Tags = { { _tagName, _messageType } } },
                cancellationToken
            );
        }
        finally
        {
            if (_isCommand)
            {
                commandMetrics.CommandHandlingEnd(_handlerName, startingTimestamp);
            }
            else
            {
                queryMetrics.QueryHandlingEnd(_handlerName, startingTimestamp);
            }
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "[{Behavior}] handle request={RequestData} and response={ResponseData}"
    )]
    private static partial void LogHandlingRequest(
        ILogger logger,
        string behavior,
        string requestData,
        string responseData
    );
}
