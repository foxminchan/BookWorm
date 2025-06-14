using BookWorm.Chassis.ActivityScope;
using BookWorm.Chassis.EventBus.Helpers;
using MassTransit;

namespace BookWorm.Chassis.EventBus.Filters;

public sealed class SendEnricherFilter<T>(IActivityScope activityScope) : IFilter<SendContext<T>>
    where T : class
{
    public async Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        var activityName = TelemetryEnrichmentHelper.GetActivityName<T>("Send");

        await activityScope.Run(
            activityName,
            async (activity, _) =>
            {
                // Enrich with common telemetry tags
                TelemetryEnrichmentHelper.EnrichWithCommonTags(activity, context, "send");

                // Add send-specific telemetry tags
                TelemetryEnrichmentHelper.EnrichWithSendSpecificTags(activity, context);

                // Continue to the next filter in the pipeline
                await next.Send(context).ConfigureAwait(false);
            },
            context.CancellationToken
        );
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("sendEnricher");
    }
}
