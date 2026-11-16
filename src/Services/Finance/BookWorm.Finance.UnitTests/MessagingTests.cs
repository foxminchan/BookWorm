using System.Text.Json;
using BookWorm.Chassis.EventBus.Kafka;
using BookWorm.Chassis.EventBus.Wolverine;
using BookWorm.Contracts;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Finance.UnitTests;

public sealed class MessagingTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    [Test]
    public void GivenCloudEventEnvelope_WhenParsing_ThenShouldExtractDataPayload()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            "John Doe",
            "john@example.com",
            42.50m
        );
        var json = JsonSerializer.Serialize(
            new
            {
                specversion = "1.0",
                type = "test",
                data = @event,
            },
            _jsonOptions
        );

        var parsed = CloudEventsMessageParser.Deserialize<UserCheckedOutIntegrationEvent>(json);

        parsed.OrderId.ShouldBe(orderId);
        parsed.BasketId.ShouldBe(basketId);
        parsed.FullName.ShouldBe("John Doe");
        parsed.Email.ShouldBe("john@example.com");
        parsed.TotalMoney.ShouldBe(42.50m);
    }

    [Test]
    public void GivenRawJson_WhenParsing_ThenShouldDeserializeDirectly()
    {
        var orderId = Guid.CreateVersion7();
        var @event = new BasketDeletedCompleteIntegrationEvent(
            orderId,
            Guid.CreateVersion7(),
            10.00m
        );
        var json = JsonSerializer.Serialize(@event, _jsonOptions);

        var parsed = CloudEventsMessageParser.Deserialize<BasketDeletedCompleteIntegrationEvent>(
            json
        );

        parsed.OrderId.ShouldBe(orderId);
        parsed.TotalMoney.ShouldBe(10.00m);
    }

    [Test]
    public void GivenEnvironment_WhenResolvingTopics_ThenShouldMatchWolverineTopicConvention()
    {
        var env = new Mock<IHostEnvironment>();
        env.Setup(e => e.ApplicationName).Returns("BookWorm.Finance");
        env.Setup(e => e.EnvironmentName).Returns("Development");

        KafkaTopicRouter
            .CreateTopicName<UserCheckedOutIntegrationEvent>(env.Object)
            .ShouldBe("bookworm.development.ordering.user-checked-out");
        KafkaTopicRouter
            .CreateTopicName<BasketDeletedCompleteIntegrationEvent>(env.Object)
            .ShouldBe("bookworm.development.basket.delete-complete");
        KafkaTopicRouter
            .CreateTopicName<BasketDeletedFailedIntegrationEvent>(env.Object)
            .ShouldBe("bookworm.development.basket.delete-failed");
        KafkaTopicRouter
            .CreateTopicName<OrderStatusChangedToCompleteIntegrationEvent>(env.Object)
            .ShouldBe("bookworm.development.ordering.order-status-change-to-complete");
        KafkaTopicRouter
            .CreateTopicName<OrderStatusChangedToCancelIntegrationEvent>(env.Object)
            .ShouldBe("bookworm.development.ordering.order-status-change-to-cancel");
        KafkaTopicRouter
            .CreateTopicName<PlaceOrderCommand>(env.Object)
            .ShouldBe("bookworm.development.finance.place-order");
    }
}
