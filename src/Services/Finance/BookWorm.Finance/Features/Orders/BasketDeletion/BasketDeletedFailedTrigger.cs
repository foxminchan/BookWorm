using Microsoft.Azure.Functions.Worker;

namespace BookWorm.Finance.Features.Orders.BasketDeletion;

public sealed class BasketDeletedFailedTrigger
{
    [Function(nameof(BasketDeletedFailedTrigger))]
    public Task RunAsync(
        [KafkaTrigger(
            "%BrokerList%",
            "%KafkaTopics:BasketDeletedFailed%",
            ConsumerGroup = Services.Finance
        )]
            string message,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellationToken
    )
    {
        var @event = CloudEventsMessageParser.Deserialize(
            message,
            FinanceEventSerializationContext.Default.BasketDeletedFailedIntegrationEvent
        );

        return client.RaiseEventAsync(
            @event.OrderId.ToString(),
            OrderOrchestrationExternalEvents.BasketDeletedFailed,
            @event,
            cancellationToken
        );
    }
}
