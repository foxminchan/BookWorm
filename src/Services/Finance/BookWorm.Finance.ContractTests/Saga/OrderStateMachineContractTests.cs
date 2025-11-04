using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Finance.Saga;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Finance.ContractTests.Saga;

public sealed class OrderStateMachineContractTests : SnapshotTestBase
{
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;
    private ISagaStateMachineTestHarness<OrderStateMachine, OrderState> _sagaHarness = null!;

    [Before(Test)]
    public async Task Setup()
    {
        var settings = new OrderStateMachineSettings
        {
            MaxAttempts = 3,
            MaxRetryTimeout = TimeSpan.FromMinutes(30),
        };

        _provider = new ServiceCollection()
            .AddSingleton(settings)
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>();
                cfg.SetTestTimeouts(testInactivityTimeout: TimeSpan.FromSeconds(60));
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        _harness.TestInactivityTimeout = TimeSpan.FromSeconds(60);

        await _harness.Start();
        _sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
    }

    [After(Test)]
    public async Task TearDown()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Test]
    public async Task GivenUserCheckedOutEvent_WhenPublished_ThenSagaShouldConsumeAndTransitionToPlacedState()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const string fullName = "John Doe";
        const string email = "john.doe@example.com";
        const decimal totalMoney = 99.99m;

        var @event = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            fullName,
            email,
            totalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Wait for saga to process
        await Task.Delay(500);

        // Assert
        await VerifySnapshot(new { harness = _harness, sagaHarness = _sagaHarness });
    }

    [Test]
    public async Task GivenBasketDeletedCompleteEvent_WhenPublished_ThenSagaShouldPublishDeleteBasketCompleteCommand()
    {
        // Arrange - Initialize saga to Placed state first
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const string fullName = "John Doe";
        const string email = "john.doe@example.com";
        const decimal totalMoney = 99.99m;

        var initialEvent = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            fullName,
            email,
            totalMoney
        );

        await _harness.Bus.Publish(initialEvent);
        await Task.Delay(500);

        var basketDeletedEvent = new BasketDeletedCompleteIntegrationEvent(
            orderId,
            basketId,
            totalMoney
        );

        // Act
        await _harness.Bus.Publish(basketDeletedEvent);

        // Wait for saga to process
        await Task.Delay(500);

        // Assert
        await VerifySnapshot(new { harness = _harness, sagaHarness = _sagaHarness });
    }

    [Test]
    public async Task GivenBasketDeletedFailedEvent_WhenPublished_ThenSagaShouldTransitionToFailedStateAndPublishFailedCommand()
    {
        // Arrange - Initialize saga to Placed state first
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const string fullName = "John Doe";
        const string email = "john.doe@example.com";
        const decimal totalMoney = 99.99m;

        var initialEvent = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            fullName,
            email,
            totalMoney
        );

        await _harness.Bus.Publish(initialEvent);
        await Task.Delay(500);

        var basketDeletedFailedEvent = new BasketDeletedFailedIntegrationEvent(
            orderId,
            basketId,
            email,
            totalMoney
        );

        // Act
        await _harness.Bus.Publish(basketDeletedFailedEvent);

        // Wait for saga to process
        await Task.Delay(500);

        // Assert
        await VerifySnapshot(new { harness = _harness, sagaHarness = _sagaHarness });
    }

    [Test]
    public async Task GivenOrderCompletedEvent_WhenPublished_ThenSagaShouldTransitionToCompletedStateAndFinalize()
    {
        // Arrange - Initialize saga to Placed state first
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const string fullName = "John Doe";
        const string email = "john.doe@example.com";
        const decimal totalMoney = 99.99m;

        var initialEvent = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            fullName,
            email,
            totalMoney
        );

        await _harness.Bus.Publish(initialEvent);
        await Task.Delay(500);

        var orderCompletedEvent = new OrderStatusChangedToCompleteIntegrationEvent(
            orderId,
            basketId,
            fullName,
            email,
            totalMoney
        );

        // Act
        await _harness.Bus.Publish(orderCompletedEvent);

        // Wait for saga to process and finalize
        await Task.Delay(500);

        // Assert
        await VerifySnapshot(new { harness = _harness, sagaHarness = _sagaHarness });
    }

    [Test]
    public async Task GivenOrderCancelledEvent_WhenPublished_ThenSagaShouldTransitionToCancelledStateAndFinalize()
    {
        // Arrange - Initialize saga to Placed state first
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const string fullName = "John Doe";
        const string email = "john.doe@example.com";
        const decimal totalMoney = 99.99m;

        var initialEvent = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            fullName,
            email,
            totalMoney
        );

        await _harness.Bus.Publish(initialEvent);
        await Task.Delay(500);

        var orderCancelledEvent = new OrderStatusChangedToCancelIntegrationEvent(
            orderId,
            basketId,
            fullName,
            email,
            totalMoney
        );

        // Act
        await _harness.Bus.Publish(orderCancelledEvent);

        // Wait for saga to process and finalize
        await Task.Delay(500);

        // Assert
        await VerifySnapshot(new { harness = _harness, sagaHarness = _sagaHarness });
    }
}
