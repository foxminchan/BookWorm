using BookWorm.Chassis.ActivityScope;
using BookWorm.Chassis.OpenTelemetry;
using MassTransit;

namespace BookWorm.Chassis.EventBus.Filters;

public sealed class ConsumeEnricherFilter<T>(IActivityScope activityScope)
    : IFilter<ConsumeContext<T>>
    where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var activityName = $"MassTransit.Consume.{context.Message.GetType().Name}";

        await activityScope.Run(
            activityName,
            async (activity, _) =>
            {
                // Enrich the activity with message context information
                activity?.SetTag(TelemetryTags.Messaging.System, "rabbitmq");
                activity?.SetTag(TelemetryTags.Messaging.Operation, "receive");
                activity?.SetTag(TelemetryTags.Messaging.MessageId, context.MessageId?.ToString());
                activity?.SetTag(
                    TelemetryTags.Messaging.ConversationId,
                    context.ConversationId?.ToString()
                );
                activity?.SetTag(
                    TelemetryTags.Messaging.CorrelationId,
                    context.CorrelationId?.ToString()
                );
                activity?.SetTag(
                    TelemetryTags.Messaging.MessageType,
                    context.Message.GetType().FullName
                );
                activity?.SetTag(
                    TelemetryTags.Messaging.Destination,
                    context.DestinationAddress?.ToString()
                );
                activity?.SetTag(
                    TelemetryTags.Messaging.SourceAddress,
                    context.SourceAddress?.ToString()
                );

                // Add request ID if available
                if (context.RequestId.HasValue)
                {
                    activity?.SetTag(
                        TelemetryTags.Messaging.RequestId,
                        context.RequestId.ToString()
                    );
                }

                // Add consumer-specific information
                activity?.SetTag(
                    TelemetryTags.Messaging.Consumer,
                    context.ReceiveContext.InputAddress.ToString()
                );

                // Add timing information
                if (context.SentTime.HasValue)
                {
                    activity?.SetTag(
                        TelemetryTags.Messaging.SentTime,
                        context.SentTime.Value.ToString("O")
                    );
                }

                // Continue to the next filter in the pipeline
                await next.Send(context).ConfigureAwait(false);
            },
            context.CancellationToken
        );
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("consumeEnricher");
    }
}
