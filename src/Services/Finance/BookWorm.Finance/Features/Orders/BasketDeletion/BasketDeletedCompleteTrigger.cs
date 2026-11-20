using Microsoft.Azure.Functions.Worker;

namespace BookWorm.Finance.Features.Orders.BasketDeletion;

public sealed class BasketDeletedCompleteTrigger
{
    [Function(nameof(BasketDeletedCompleteTrigger))]
    public Task RunAsync(
        [KafkaTrigger(
            "%BrokerList%",
            "%KafkaTopics:BasketDeletedComplete%",
            ConsumerGroup = Services.Finance
        )]
            string message,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellationToken
    )
    {
        var @event = CloudEventsMessageParser.Deserialize(
            message,
            FinanceEventSerializationContext.Default.BasketDeletedCompleteIntegrationEvent
        );

        return client.RaiseEventAsync(
            @event.OrderId.ToString(),
            OrderOrchestrationExternalEvents.BasketDeletedComplete,
            @event,
            cancellationToken
        );
    }
}
