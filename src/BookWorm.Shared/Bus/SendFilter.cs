namespace BookWorm.Shared.Bus;

public sealed class SendFilter<T>(IActivityScope activityScope) : IFilter<SendContext<T>> where T : class
{
    public async Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        var name = $"{context.Message.GetType().FullName}-Sender";

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
                    ["correlation-id"] = context.CorrelationId?.ToString()
                }
            },
            default
        );
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("activityScope");
    }
}
