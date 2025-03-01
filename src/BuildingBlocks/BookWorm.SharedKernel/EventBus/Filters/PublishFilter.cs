using BookWorm.SharedKernel.ActivityScope;
using MassTransit;

namespace BookWorm.SharedKernel.EventBus.Filters;

public sealed class PublishFilter<T>(IActivityScope activityScope) : IFilter<PublishContext<T>>
    where T : class
{
    public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        var name = $"{context.Message.GetType().FullName}-Publisher";

        await activityScope.Run(
            name,
            async (_, _) => await next.Send(context),
            new()
            {
                Tags =
                {
                    ["message-id"] = context.MessageId?.ToString(),
                    ["message-type"] = context.Message.GetType().FullName,
                    ["destination-address"] = context.DestinationAddress?.ToString(),
                    ["source-address"] = context.SourceAddress?.ToString(),
                    ["correlation-id"] = context.CorrelationId?.ToString(),
                },
            },
            default
        );
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("activityScope");
    }
}
