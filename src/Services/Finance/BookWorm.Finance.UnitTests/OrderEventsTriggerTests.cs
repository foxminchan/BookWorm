using System.Text.Json;
using BookWorm.Contracts;
using BookWorm.Finance.Features.Orders.BasketDeletion;
using BookWorm.Finance.Features.Orders.Orchestration;
using BookWorm.Finance.Features.Orders.Orchestration.Models;
using BookWorm.Finance.Features.Orders.OrderCancellation;
using BookWorm.Finance.Features.Orders.OrderCompletion;
using BookWorm.Finance.Features.Orders.PlaceOrder;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BookWorm.Finance.UnitTests;

public sealed class OrderEventsTriggerTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private readonly Mock<DurableTaskClient> _clientMock = new("test");

    private readonly OrderStateMachineSettings _settings = new()
    {
        MaxAttempts = 3,
        MaxRetryTimeout = TimeSpan.FromMinutes(30),
    };

    [Test]
    public async Task GivenUserCheckedOutEvent_WhenNotYetStarted_ThenShouldScheduleOrchestrationWithOrderId()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            "John Doe",
            "john@example.com",
            99.99m
        );

        _clientMock
            .Setup(c => c.GetInstanceAsync(orderId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrchestrationMetadata?)null);

        _clientMock
            .Setup(c =>
                c.ScheduleNewOrchestrationInstanceAsync(
                    nameof(OrderOrchestrator),
                    It.IsAny<object?>(),
                    It.IsAny<StartOrchestrationOptions?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(orderId.ToString());

        var trigger = CreateUserCheckedOutTrigger();
        await trigger.RunAsync(
            WrapInCloudEvent(@event),
            _clientMock.Object,
            CancellationToken.None
        );

        _clientMock.Verify(
            c =>
                c.ScheduleNewOrchestrationInstanceAsync(
                    nameof(OrderOrchestrator),
                    It.Is<OrderOrchestrationInput>(i =>
                        i.OrderId == orderId
                        && i.BasketId == basketId
                        && i.FullName == "John Doe"
                        && i.Email == "john@example.com"
                        && i.TotalMoney == 99.99m
                        && i.MaxAttempts == 3
                        && i.MaxRetryTimeout == TimeSpan.FromMinutes(30)
                    ),
                    It.Is<StartOrchestrationOptions>(o => o.InstanceId == orderId.ToString()),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenDuplicateUserCheckedOutEvent_WhenInstanceExists_ThenShouldNotStartSecondOrchestration()
    {
        var orderId = Guid.CreateVersion7();
        var @event = new UserCheckedOutIntegrationEvent(
            orderId,
            Guid.CreateVersion7(),
            "John Doe",
            "john@example.com",
            99.99m
        );

        _clientMock
            .Setup(c => c.GetInstanceAsync(orderId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrchestrationMetadata(nameof(OrderOrchestrator), orderId.ToString()));

        var trigger = CreateUserCheckedOutTrigger();
        await trigger.RunAsync(
            WrapInCloudEvent(@event),
            _clientMock.Object,
            CancellationToken.None
        );

        _clientMock.Verify(
            c =>
                c.ScheduleNewOrchestrationInstanceAsync(
                    It.IsAny<TaskName>(),
                    It.IsAny<object?>(),
                    It.IsAny<StartOrchestrationOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Test]
    public async Task GivenBasketDeletedCompleteEvent_WhenTriggered_ThenShouldRaiseExternalEvent()
    {
        var orderId = Guid.CreateVersion7();
        var @event = new BasketDeletedCompleteIntegrationEvent(
            orderId,
            Guid.CreateVersion7(),
            75.25m
        );

        await new BasketDeletedCompleteTrigger().RunAsync(
            WrapInCloudEvent(@event),
            _clientMock.Object,
            CancellationToken.None
        );

        _clientMock.Verify(
            c =>
                c.RaiseEventAsync(
                    orderId.ToString(),
                    OrderOrchestrationExternalEvents.BasketDeletedComplete,
                    It.Is<BasketDeletedCompleteIntegrationEvent>(e =>
                        e.OrderId == orderId && e.TotalMoney == 75.25m
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenBasketDeletedFailedEvent_WhenTriggered_ThenShouldRaiseExternalEvent()
    {
        var orderId = Guid.CreateVersion7();
        var @event = new BasketDeletedFailedIntegrationEvent(
            orderId,
            Guid.CreateVersion7(),
            "user@example.com",
            120.00m
        );

        await new BasketDeletedFailedTrigger().RunAsync(
            WrapInCloudEvent(@event),
            _clientMock.Object,
            CancellationToken.None
        );

        _clientMock.Verify(
            c =>
                c.RaiseEventAsync(
                    orderId.ToString(),
                    OrderOrchestrationExternalEvents.BasketDeletedFailed,
                    It.Is<BasketDeletedFailedIntegrationEvent>(e => e.OrderId == orderId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenOrderCompletedEvent_WhenTriggered_ThenShouldRaiseExternalEvent()
    {
        var orderId = Guid.CreateVersion7();
        var @event = new OrderStatusChangedToCompleteIntegrationEvent(
            orderId,
            Guid.CreateVersion7(),
            "Jane Doe",
            "jane@example.com",
            210.00m
        );

        await new OrderCompletedTrigger().RunAsync(
            WrapInCloudEvent(@event),
            _clientMock.Object,
            CancellationToken.None
        );

        _clientMock.Verify(
            c =>
                c.RaiseEventAsync(
                    orderId.ToString(),
                    OrderOrchestrationExternalEvents.OrderCompleted,
                    It.Is<OrderStatusChangedToCompleteIntegrationEvent>(e => e.OrderId == orderId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenOrderCancelledEvent_WhenTriggered_ThenShouldRaiseExternalEvent()
    {
        var orderId = Guid.CreateVersion7();
        var @event = new OrderStatusChangedToCancelIntegrationEvent(
            orderId,
            Guid.CreateVersion7(),
            "Bob Doe",
            "bob@example.com",
            180.00m
        );

        await new OrderCancelledTrigger().RunAsync(
            WrapInCloudEvent(@event),
            _clientMock.Object,
            CancellationToken.None
        );

        _clientMock.Verify(
            c =>
                c.RaiseEventAsync(
                    orderId.ToString(),
                    OrderOrchestrationExternalEvents.OrderCancelled,
                    It.Is<OrderStatusChangedToCancelIntegrationEvent>(e => e.OrderId == orderId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    private UserCheckedOutTrigger CreateUserCheckedOutTrigger()
    {
        return new(Options.Create(_settings), NullLogger<UserCheckedOutTrigger>.Instance);
    }

    private static string WrapInCloudEvent<T>(T payload)
    {
        return JsonSerializer.Serialize(new { specversion = "1.0", data = payload }, _jsonOptions);
    }
}
