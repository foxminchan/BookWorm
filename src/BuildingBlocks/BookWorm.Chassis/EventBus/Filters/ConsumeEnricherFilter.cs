using BookWorm.Chassis.ActivityScope;
using BookWorm.Chassis.EventBus.Helpers;
using MassTransit;

namespace BookWorm.Chassis.EventBus.Filters;

public sealed class ConsumeEnricherFilter<T>(IActivityScope activityScope)
    : IFilter<ConsumeContext<T>>
    where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var activityName = TelemetryEnrichmentHelper.GetActivityName<T>("Consume");

        await activityScope.Run(
            activityName,
            async (activity, _) =>
            {
                TelemetryEnrichmentHelper.EnrichWithCommonTags(activity, context, "receive");
                TelemetryEnrichmentHelper.EnrichWithConsumeSpecificTags(activity, context);

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
