using System.Reflection;
using BookWorm.Chassis.ActivityScope;
using BookWorm.Chassis.Command;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.Query;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.Pipelines;

public sealed class ActivityBehavior<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> outerHandler,
    IActivityScope activityScope,
    CommandHandlerMetrics commandMetrics,
    QueryHandlerMetrics queryMetrics,
    ILogger<ActivityBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "[{Behavior}] handle request={RequestData} and response={ResponseData}",
                nameof(ActivityBehavior<TRequest, TResponse>),
                typeof(TRequest).FullName,
                typeof(TResponse).FullName
            );
        }

        var attr = outerHandler.GetType().GetCustomAttribute<IgnoreOTelOnHandlerAttribute>();

        if (attr is not null)
        {
            return await next(cancellationToken);
        }

        var handlerName = outerHandler.GetType().Name;
        var queryName = typeof(TRequest).Name;
        var activityName = $"{handlerName}/{queryName}";

        var isCommand = queryName.ToLowerInvariant().EndsWith(nameof(Command).ToLowerInvariant());
        var tagName = isCommand ? TelemetryTags.Commands.Command : TelemetryTags.Queries.Query;

        var startingTimestamp = isCommand
            ? commandMetrics.CommandHandlingStart(handlerName)
            : queryMetrics.QueryHandlingStart(handlerName);

        try
        {
            return await activityScope.Run(
                activityName,
                async (_, ct) => await next(ct),
                new() { Tags = { { tagName, queryName } } },
                cancellationToken
            );
        }
        finally
        {
            if (isCommand)
            {
                commandMetrics.CommandHandlingEnd(handlerName, startingTimestamp);
            }
            else
            {
                queryMetrics.QueryHandlingEnd(handlerName, startingTimestamp);
            }
        }
    }
}
