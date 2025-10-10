using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.CQRS.Pipelines;

public class ValidationBehavior<TRequest, TResponse>(
    IActivityScope activityScope,
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        const string behavior = nameof(ValidationBehavior<,>);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "[{Behavior}] handle request={RequestData} and response={ResponseData}",
                behavior,
                typeof(TRequest).FullName,
                typeof(TResponse).FullName
            );
        }

        if (!validators.Any())
        {
            return await next(message, cancellationToken);
        }

        var context = new ValidationContext<TRequest>(message);

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
            async (_, ct) => await next(message, ct),
            new() { Tags = { { TelemetryTags.Validator.Validation, queryName } } },
            cancellationToken
        );
    }
}
