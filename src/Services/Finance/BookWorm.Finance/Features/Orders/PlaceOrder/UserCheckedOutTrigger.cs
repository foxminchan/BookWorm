using BookWorm.Contracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;

namespace BookWorm.Finance.Features.Orders.PlaceOrder;

public sealed class UserCheckedOutTrigger(
    IOptions<OrderStateMachineSettings> settings,
    ILogger<UserCheckedOutTrigger> logger
)
{
    [Function(nameof(UserCheckedOutTrigger))]
    public async Task RunAsync(
        [KafkaTrigger(
            "%BrokerList%",
            "%KafkaTopics:UserCheckedOut%",
            ConsumerGroup = Services.Finance
        )]
            string message,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellationToken
    )
    {
        var @event = CloudEventsMessageParser.Deserialize<UserCheckedOutIntegrationEvent>(message);
        var instanceId = @event.OrderId.ToString();

        var existing = await client.GetInstanceAsync(instanceId, cancellationToken);
        if (existing is not null)
        {
            logger.LogWarning(
                "Duplicate checkout event ignored for order {OrderId}",
                @event.OrderId
            );
            return;
        }

        var currentSettings = settings.Value;
        var input = new OrderOrchestrationInput(
            @event.OrderId,
            @event.BasketId,
            @event.FullName,
            @event.Email,
            @event.TotalMoney,
            currentSettings.MaxAttempts,
            currentSettings.MaxRetryTimeout
        );

        await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(OrderOrchestrator),
            input,
            new() { InstanceId = instanceId },
            cancellationToken
        );

        logger.LogInformation(
            "Started {OrchestratorName} for order {OrderId}",
            nameof(OrderOrchestrator),
            @event.OrderId
        );
    }
}
