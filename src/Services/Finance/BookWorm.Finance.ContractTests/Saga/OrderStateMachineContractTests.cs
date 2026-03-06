using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Finance.Saga;
using BookWorm.Finance.Saga.Activities;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Finance.ContractTests.Saga;

public sealed class OrderStateMachineContractTests
{
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;
    private ISagaStateMachineTestHarness<OrderStateMachine, OrderState> _sagaHarness = null!;

    [Before(Test)]
    public async Task SetUpAsync()
    {
        var settings = new OrderStateMachineSettings
        {
            MaxAttempts = 3,
            MaxRetryTimeout = TimeSpan.FromMinutes(30),
        };

        _provider = new ServiceCollection()
            .AddSingleton(settings)
            .AddScoped<PlaceOrderActivity>()
            .AddScoped<CancelOrderActivity>()
            .AddScoped<CompleteOrderActivity>()
            .AddScoped<HandleBasketDeletedActivity>()
            .AddScoped<HandleBasketDeleteFailedActivity>()
            .AddTelemetryListener()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>();
                cfg.SetTestTimeouts(testInactivityTimeout: TimeSpan.FromSeconds(60));
            })
            .BuildServiceProvider(true);

        _harness = await _provider.StartTestHarness();
        _harness.TestInactivityTimeout = TimeSpan.FromSeconds(60);

        _sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
    }

    [After(Test)]
    public async Task TearDownAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    private const string TestFullName = "John Doe";
    private const string TestEmail = "john.doe@example.com";
    private const decimal TestTotalMoney = 99.99m;

    private async Task<(Guid OrderId, Guid BasketId)> InitializeSagaAsync()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var initialEvent = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            TestFullName,
            TestEmail,
            TestTotalMoney
        );
        await _harness.Bus.Publish(initialEvent);
        await _sagaHarness.Consumed.Any<UserCheckedOutIntegrationEvent>();
        return (orderId, basketId);
    }

    [Test]
    public async Task GivenUserCheckedOutEvent_WhenPublished_ThenSagaShouldConsumeAndTransitionToPlacedState()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();

        var @event = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            TestFullName,
            TestEmail,
            TestTotalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Wait for saga to process
        await _sagaHarness.Consumed.Any<UserCheckedOutIntegrationEvent>();

        // Assert
        await SnapshotTestHelper.Verify(new { harness = _harness, sagaHarness = _sagaHarness });
    }

    [Test]
    public async Task GivenBasketDeletedCompleteEvent_WhenPublished_ThenSagaShouldPublishDeleteBasketCompleteCommand()
    {
        // Arrange - Initialize saga to Placed state
        var (orderId, basketId) = await InitializeSagaAsync();

        var basketDeletedEvent = new BasketDeletedCompleteIntegrationEvent(
            orderId,
            basketId,
            TestTotalMoney
        );

        // Act
        await _harness.Bus.Publish(basketDeletedEvent);

        // Wait for saga to process
        await _sagaHarness.Consumed.Any<BasketDeletedCompleteIntegrationEvent>();

        // Assert
        await SnapshotTestHelper.Verify(new { harness = _harness, sagaHarness = _sagaHarness });
    }

    [Test]
    public async Task GivenBasketDeletedFailedEvent_WhenPublished_ThenSagaShouldTransitionToFailedStateAndPublishFailedCommand()
    {
        // Arrange - Initialize saga to Placed state
        var (orderId, basketId) = await InitializeSagaAsync();

        var basketDeletedFailedEvent = new BasketDeletedFailedIntegrationEvent(
            orderId,
            basketId,
            TestEmail,
            TestTotalMoney
        );

        // Act
        await _harness.Bus.Publish(basketDeletedFailedEvent);

        // Wait for saga to process
        await _sagaHarness.Consumed.Any<BasketDeletedFailedIntegrationEvent>();

        // Assert
        await SnapshotTestHelper.Verify(new { harness = _harness, sagaHarness = _sagaHarness });
    }

    [Test]
    public async Task GivenOrderCompletedEvent_WhenPublished_ThenSagaShouldTransitionToCompletedStateAndFinalize()
    {
        // Arrange - Initialize saga to Placed state
        var (orderId, basketId) = await InitializeSagaAsync();

        var orderCompletedEvent = new OrderStatusChangedToCompleteIntegrationEvent(
            orderId,
            basketId,
            TestFullName,
            TestEmail,
            TestTotalMoney
        );

        // Act
        await _harness.Bus.Publish(orderCompletedEvent);

        // Wait for saga to process and finalize
        await _sagaHarness.Consumed.Any<OrderStatusChangedToCompleteIntegrationEvent>();

        // Assert
        await SnapshotTestHelper.Verify(new { harness = _harness, sagaHarness = _sagaHarness });
    }

    [Test]
    public async Task GivenOrderCancelledEvent_WhenPublished_ThenSagaShouldTransitionToCancelledStateAndFinalize()
    {
        // Arrange - Initialize saga to Placed state
        var (orderId, basketId) = await InitializeSagaAsync();

        var orderCancelledEvent = new OrderStatusChangedToCancelIntegrationEvent(
            orderId,
            basketId,
            TestFullName,
            TestEmail,
            TestTotalMoney
        );

        // Act
        await _harness.Bus.Publish(orderCancelledEvent);

        // Wait for saga to process and finalize
        await _sagaHarness.Consumed.Any<OrderStatusChangedToCancelIntegrationEvent>();

        // Assert
        await SnapshotTestHelper.Verify(new { harness = _harness, sagaHarness = _sagaHarness });
    }
}
