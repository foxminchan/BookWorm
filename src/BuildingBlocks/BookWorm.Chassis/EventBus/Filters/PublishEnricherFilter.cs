using BookWorm.Chassis.ActivityScope;
using BookWorm.Chassis.OpenTelemetry;
using MassTransit;

namespace BookWorm.Chassis.EventBus.Filters;

public sealed class PublishEnricherFilter<T>(IActivityScope activityScope)
    : IFilter<PublishContext<T>>
    where T : class
{
    public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        var activityName = $"MassTransit.Publish.{context.Message.GetType().Name}";

        await activityScope.Run(
            activityName,
            async (activity, _) =>
            {
                // Enrich the activity with message context information
                activity?.SetTag(TelemetryTags.Messaging.System, "rabbitmq");
                activity?.SetTag(TelemetryTags.Messaging.Operation, "publish");
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

                // Continue to the next filter in the pipeline
                await next.Send(context).ConfigureAwait(false);
            },
            context.CancellationToken
        );
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("publishEnricher");
    }
}
