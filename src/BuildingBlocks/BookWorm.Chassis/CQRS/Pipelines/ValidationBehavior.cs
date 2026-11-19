using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using FluentValidation;
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

        var validatorNames = string.Join(", ", validators.Select(v => v.GetType().Name));
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
