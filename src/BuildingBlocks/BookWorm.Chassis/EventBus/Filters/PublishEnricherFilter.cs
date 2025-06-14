using BookWorm.Chassis.ActivityScope;
using BookWorm.Chassis.EventBus.Helpers;
using MassTransit;

namespace BookWorm.Chassis.EventBus.Filters;

public sealed class PublishEnricherFilter<T>(IActivityScope activityScope)
    : IFilter<PublishContext<T>>
    where T : class
{
    public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        var activityName = TelemetryEnrichmentHelper.GetActivityName<T>("Publish");

        await activityScope.Run(
            activityName,
            async (activity, _) =>
            {
                TelemetryEnrichmentHelper.EnrichWithCommonTags(activity, context, "publish");

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
