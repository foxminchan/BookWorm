using System.Text.Json;
using BookWorm.Contracts;
using BookWorm.Finance.Features.Orders.BasketDeletion;
using BookWorm.Finance.Features.Orders.Orchestration;
using BookWorm.Finance.Features.Orders.OrderCancellation;
using BookWorm.Finance.Features.Orders.OrderCompletion;
using BookWorm.Finance.Features.Orders.PlaceOrder;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BookWorm.Finance.ContractTests.Orchestration;

[Category("PactConsumer")]
public sealed class OrderOrchestrationContractTests
{
    private const string TestFullName = "John Doe";
    private const string TestEmail = "john.doe@example.com";
    private const decimal TestTotalMoney = 99.99m;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private Mock<DurableTaskClient> _clientMock = null!;
    private UserCheckedOutTrigger _userCheckedOutTrigger = null!;

    [Before(Test)]
    public void SetUp()
    {
        var settings = new OrderStateMachineSettings
        {
            MaxAttempts = 3,
            MaxRetryTimeout = TimeSpan.FromMinutes(30),
        };
        _clientMock = new("pact-test");
        _clientMock
            .Setup(c =>
                c.ScheduleNewOrchestrationInstanceAsync(
                    It.IsAny<TaskName>(),
                    It.IsAny<object?>(),
                    It.IsAny<StartOrchestrationOptions?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(string.Empty);

        _userCheckedOutTrigger = new(
            Options.Create(settings),
            NullLogger<UserCheckedOutTrigger>.Instance
        );
    }

    [Test]
    public async Task GivenUserCheckedOutEvent_WhenPublished_ThenTriggerShouldStartOrchestration()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            TestFullName,
            TestEmail,
            TestTotalMoney
        );

        await PactTests.Helper.VerifyConsumerMessageAsync(
            "Finance",
            "Ordering",
            @event,
            pactEvent =>
                _userCheckedOutTrigger.RunAsync(
                    WrapInCloudEvent(pactEvent),
                    _clientMock.Object,
                    CancellationToken.None
                )
        );
    }

    [Test]
    public async Task GivenBasketDeletedCompleteEvent_WhenHandled_ThenTriggerShouldRaiseExternalEvent()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new BasketDeletedCompleteIntegrationEvent(orderId, basketId, TestTotalMoney);

        await PactTests.Helper.VerifyConsumerMessageAsync("Finance", "Basket", @event, pactEvent =>
            new BasketDeletedCompleteTrigger().RunAsync(
                WrapInCloudEvent(pactEvent),
                _clientMock.Object,
                CancellationToken.None
            )
        );
    }

    [Test]
    public async Task GivenBasketDeletedFailedEvent_WhenHandled_ThenTriggerShouldRaiseExternalEvent()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new BasketDeletedFailedIntegrationEvent(
            orderId,
            basketId,
            TestEmail,
            TestTotalMoney
        );

        await PactTests.Helper.VerifyConsumerMessageAsync("Finance", "Basket", @event, pactEvent =>
            new BasketDeletedFailedTrigger().RunAsync(
                WrapInCloudEvent(pactEvent),
                _clientMock.Object,
                CancellationToken.None
            )
        );
    }

    [Test]
    public async Task GivenOrderCompletedEvent_WhenHandled_ThenTriggerShouldRaiseExternalEvent()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new OrderStatusChangedToCompleteIntegrationEvent(
            orderId,
            basketId,
            TestFullName,
            TestEmail,
            TestTotalMoney
        );

        await PactTests.Helper.VerifyConsumerMessageAsync(
            "Finance",
            "Ordering",
            @event,
            pactEvent =>
                new OrderCompletedTrigger().RunAsync(
                    WrapInCloudEvent(pactEvent),
                    _clientMock.Object,
                    CancellationToken.None
                )
        );
    }

    [Test]
    public async Task GivenOrderCancelledEvent_WhenHandled_ThenTriggerShouldRaiseExternalEvent()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new OrderStatusChangedToCancelIntegrationEvent(
            orderId,
            basketId,
            TestFullName,
            TestEmail,
            TestTotalMoney
        );

        await PactTests.Helper.VerifyConsumerMessageAsync(
            "Finance",
            "Ordering",
            @event,
            pactEvent =>
                new OrderCancelledTrigger().RunAsync(
                    WrapInCloudEvent(pactEvent),
                    _clientMock.Object,
                    CancellationToken.None
                )
        );
    }

    private static string WrapInCloudEvent<T>(T payload)
    {
        return JsonSerializer.Serialize(new { specversion = "1.0", data = payload }, _jsonOptions);
    }
}
