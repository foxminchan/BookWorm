using BookWorm.Contracts;
using Microsoft.Azure.Functions.Worker;

namespace BookWorm.Finance.Features.Orders.OrderCancellation;

public sealed class OrderCancelledTrigger
{
    [Function(nameof(OrderCancelledTrigger))]
    public Task RunAsync(
        [KafkaTrigger(
            "%BrokerList%",
            "%KafkaTopics:OrderStatusChangedToCancel%",
            ConsumerGroup = Services.Finance
        )]
            string message,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellationToken
    )
    {
        var @event =
            CloudEventsMessageParser.Deserialize<OrderStatusChangedToCancelIntegrationEvent>(
                message
            );

        return client.RaiseEventAsync(
            @event.OrderId.ToString(),
            OrderOrchestrationExternalEvents.OrderCancelled,
            @event,
            cancellationToken
        );
    }
}
