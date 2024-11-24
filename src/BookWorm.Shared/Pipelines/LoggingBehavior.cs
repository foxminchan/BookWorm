namespace BookWorm.Shared.Pipelines;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        const string behavior = nameof(LoggingBehavior<TRequest, TResponse>);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "[{Behavior}] Handle request={Request} and response={Response}",
                behavior,
                typeof(TRequest).FullName,
                typeof(TResponse).FullName
            );
        }

        var start = Stopwatch.GetTimestamp();

        var response = await next();

        logger.LogInformation(
            "[{Behavior}] The request handled {RequestName} with {Response} in {ElapsedMilliseconds} ms",
            behavior,
            typeof(TRequest).Name,
            response,
            Stopwatch.GetElapsedTime(start).Milliseconds
        );

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

        return response;
    }
}
