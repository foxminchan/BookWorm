using System.Reflection;
using BookWorm.Shared.ActivityScope;
using BookWorm.Shared.Metrics;
using BookWorm.Shared.OpenTelemetry;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookWorm.Shared.Pipelines;

public sealed class MetricsBehavior<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> requestHandler,
    IActivityScope activityScope,
    CommandHandlerMetrics commandMetrics,
    QueryHandlerMetrics queryMetrics,
    ILogger<MetricsBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("[{Behavior}] handle request={RequestData} and response={ResponseData}",
            nameof(MetricsBehavior<TRequest, TResponse>), typeof(TRequest).FullName, typeof(TResponse).FullName);

        var attr = requestHandler
            .GetType().GetCustomAttribute<IgnoreOTelOnHandlerAttribute>();

        if (attr is not null)
        {
            return await next();
        }

        var handler = requestHandler.GetType().Name;
        var query = typeof(TRequest).Name;
        var activityName = $"{query}-{handler}";
        var isQuery = query.ToLowerInvariant().EndsWith(nameof(query));

        var tagName = isQuery ? TelemetryTags.Queries.Query : TelemetryTags.Commands.Command;

        var startingTimestamp = isQuery
            ? queryMetrics.QueryHandlingStart(handler)
            : commandMetrics.CommandHandlingStart(handler);

        try
        {
            return await activityScope.Run(
                activityName,
                async (_, _) => await next(),
                new() { Tags = { { tagName, query } } },
                cancellationToken
            );
        }
        finally
        {
            if (isQuery)
            {
                queryMetrics.QueryHandlingEnd(handler, startingTimestamp);
            }
            else
            {
                commandMetrics.CommandHandlingEnd(handler, startingTimestamp);
            }
        }
    }
}
