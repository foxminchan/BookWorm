namespace BookWorm.Shared.Pipelines;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IActivityScope activityScope,
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger
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
        const string behavior = nameof(ValidationBehavior<TRequest, TResponse>);

        logger.LogInformation(
            "[{Behavior}] handle request={RequestData} and response={ResponseData}",
            behavior,
            typeof(TRequest).FullName,
            typeof(TResponse).FullName
        );

        logger.LogDebug(
            "[{Behavior}] handle request={Request} with content={RequestData}",
            behavior,
            typeof(TRequest).FullName,
            JsonSerializer.Serialize(request)
        );

        var context = new ValidationContext<TRequest>(request);

        var validationResult = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var errors = validationResult
            .Where(result => !result.IsValid)
            .SelectMany(result => result.Errors)
            .Select(failure => new ValidationFailure(failure.PropertyName, failure.ErrorMessage))
            .ToList();

        if (errors.Count != 0)
        {
            throw new ValidationException(errors);
        }

        var queryName = typeof(TRequest).Name;
        var validatorNames = validators.Aggregate("", (c, x) => $"{x.GetType().Name}, {c}");
        var activityName = $"{queryName}-{validatorNames.Trim().TrimEnd(',')}";

        return await activityScope.Run(
            activityName,
            async (_, _) => await next(),
            new() { Tags = { { TelemetryTags.Validator.Validation, queryName } } },
            cancellationToken
        );
    }
}
