using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.CQRS.Pipelines;

internal sealed partial class ValidationBehavior<TMessage, TResponse>(
    IActivityScope activityScope,
    IEnumerable<IValidator<TMessage>> validators,
    ILogger<ValidationBehavior<TMessage, TResponse>> logger
) : MessagePreProcessor<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : notnull
{
    private const string Behavior = nameof(ValidationBehavior<,>);
    private static readonly string _messageType = typeof(TMessage).Name;
    private static readonly string _responseType = typeof(TResponse).Name;

    protected override async ValueTask Handle(TMessage message, CancellationToken cancellationToken)
    {
        LogHandlingRequest(logger, Behavior, _messageType, _responseType);

        var resolvedValidators = validators as IValidator<TMessage>[] ?? [.. validators];

        if (resolvedValidators.Length == 0)
        {
            return;
        }

        var context = new ValidationContext<TMessage>(message);

        ValidationResult[] validationResults =
            resolvedValidators.Length == 1
                ? [await resolvedValidators[0].ValidateAsync(context, cancellationToken)]
                : await Task.WhenAll(
                    resolvedValidators.Select(v => v.ValidateAsync(context, cancellationToken))
                );

        List<ValidationFailure>? errors = null;

        foreach (var result in validationResults)
        {
            if (result.Errors.Count != 0)
            {
                (errors ??= []).AddRange(result.Errors);
            }
        }

        if (errors is not null)
        {
            throw new ValidationException(errors);
        }

        var validatorNames = string.Join(
            ", ",
            resolvedValidators.Select(static v => v.GetType().Name)
        );
        var activityName = $"{_messageType}-{validatorNames}";

        await activityScope.Run(
            activityName,
            (_, _) => Task.CompletedTask,
            new() { Tags = { { TelemetryTags.Validator.Validation, _messageType } } },
            cancellationToken
        );
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
