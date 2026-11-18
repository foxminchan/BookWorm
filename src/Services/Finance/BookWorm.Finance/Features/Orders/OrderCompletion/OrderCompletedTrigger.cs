using BookWorm.Contracts;
using Microsoft.Azure.Functions.Worker;

namespace BookWorm.Finance.Features.Orders.OrderCompletion;

public sealed class OrderCompletedTrigger
{
    [Function(nameof(OrderCompletedTrigger))]
    public Task RunAsync(
        [KafkaTrigger(
            "%BrokerList%",
            "%KafkaTopics:OrderStatusChangedToComplete%",
            ConsumerGroup = Services.Finance
        )]
            string message,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellationToken
    )
    {
        var @event =
            CloudEventsMessageParser.Deserialize<OrderStatusChangedToCompleteIntegrationEvent>(
                message
            );

        return client.RaiseEventAsync(
            @event.OrderId.ToString(),
            OrderOrchestrationExternalEvents.OrderCompleted,
            @event,
            cancellationToken
        );
    }
}
