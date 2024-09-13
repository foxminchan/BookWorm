namespace BookWorm.Shared.Pipelines;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IActivityScope activityScope,
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        const string behavior = nameof(ValidationBehavior<TRequest, TResponse>);

        logger.LogInformation(
            "[{Behavior}] handle request={RequestData} and response={ResponseData}",
            behavior, typeof(TRequest).FullName, typeof(TResponse).FullName);

        logger.LogDebug(
            "[{Behavior}] handle request={Request} with content={RequestData}",
            behavior, typeof(TRequest).FullName, JsonSerializer.Serialize(request));

        var context = new ValidationContext<TRequest>(request);

        var validationResult = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResult.Select(
            r => r.Errors.Select(e => new ValidationFailure(e.PropertyName, e.ErrorMessage)))
            .SelectMany(f => f)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await activityScope.Run(
            behavior,
            async (_, _) => await next(),
            new()
            {
                Tags =
                {
                    { TelemetryTags.Validator.Validation, typeof(TRequest).Name },
                    { TelemetryTags.Validator.ValidationRequest, JsonSerializer.Serialize(request) },
                    { TelemetryTags.Validator.ValidationResponseType, typeof(TResponse).FullName },
                    { TelemetryTags.Validator.ValidationValidators, JsonSerializer.Serialize(validators) }
                }
            },
            cancellationToken
        );
    }
}
