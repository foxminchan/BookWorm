using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.CQRS.Pipelines;

internal sealed class ValidationBehavior<TMessage, TResponse>(
    IActivityScope activityScope,
    IEnumerable<IValidator<TMessage>> validators,
    ILogger<ValidationBehavior<TMessage, TResponse>> logger
) : MessagePreProcessor<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : notnull
{
    protected override async ValueTask Handle(TMessage message, CancellationToken cancellationToken)
    {
        const string behavior = nameof(ValidationBehavior<,>);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "[{Behavior}] handle request={RequestData} and response={ResponseData}",
                behavior,
                message.GetType().Name,
                typeof(TResponse).Name
            );
        }

        if (!validators.Any())
        {
            return;
        }

        var context = new ValidationContext<TMessage>(message);

        var validationResult = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var errors = validationResult.SelectMany(result => result.Errors).ToList();

        if (errors.Count != 0)
        {
            throw new ValidationException(errors);
        }

        var messageType = message.GetType().Name;
        var validatorNames = string.Join(", ", validators.Select(v => v.GetType().Name));
        var activityName = $"{messageType}-{validatorNames}";

        await activityScope.Run(
            activityName,
            (_, _) => Task.CompletedTask,
            new() { Tags = { { TelemetryTags.Validator.Validation, messageType } } },
            cancellationToken
        );
    }
}
