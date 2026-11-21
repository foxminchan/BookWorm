using System.Collections.Concurrent;
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

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var metadata = ActivityBehaviorMetadata.GetOrAdd(typeof(TMessage));

        LogHandlingRequest(logger, Behavior, metadata.MessageType, typeof(TResponse).Name);

        if (metadata.IgnoreOTel)
        {
            return await next(message, cancellationToken);
        }

        var startingTimestamp = metadata.IsCommand
            ? commandMetrics.CommandHandlingStart(metadata.HandlerName)
            : queryMetrics.QueryHandlingStart(metadata.HandlerName);

        try
        {
            return await activityScope.Run(
                metadata.ActivityName,
                async (_, ct) => await next(message, ct),
                new() { Tags = { { metadata.TagName, metadata.MessageType } } },
                cancellationToken
            );
        }
        finally
        {
            if (metadata.IsCommand)
            {
                commandMetrics.CommandHandlingEnd(metadata.HandlerName, startingTimestamp);
            }
            else
            {
                queryMetrics.QueryHandlingEnd(metadata.HandlerName, startingTimestamp);
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

file static class ActivityBehaviorMetadata
{
    private static readonly ConcurrentDictionary<Type, MessageMetadata> _cache = new();

    public static MessageMetadata GetOrAdd(Type messageType)
    {
        return _cache.GetOrAdd(messageType, static type =>
        {
            var typeName = type.Name;
            var handlerName = $"{typeName}Handler";
            var isCommand = typeName.EndsWith(nameof(Command), StringComparison.OrdinalIgnoreCase);

            return new(
                typeName,
                handlerName,
                $"{handlerName}/{typeName}",
                type.GetCustomAttribute<IgnoreOTelOnHandlerAttribute>() is not null,
                isCommand,
                isCommand ? TelemetryTags.Commands.Command : TelemetryTags.Queries.Query
            );
        });
    }

    public readonly record struct MessageMetadata(
        string MessageType,
        string HandlerName,
        string ActivityName,
        bool IgnoreOTel,
        bool IsCommand,
        string TagName
    );
}
