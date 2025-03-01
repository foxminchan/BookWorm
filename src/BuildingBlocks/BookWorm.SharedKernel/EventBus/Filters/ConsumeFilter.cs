using BookWorm.SharedKernel.ActivityScope;
using MassTransit;

namespace BookWorm.SharedKernel.EventBus.Filters;

public sealed class ConsumeFilter<T>(IActivityScope activityScope) : IFilter<ConsumeContext<T>>
    where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var name = $"{context.Message.GetType().FullName}-Consumer";

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
            CancellationToken.None
        );
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("activityScope");
    }
}
