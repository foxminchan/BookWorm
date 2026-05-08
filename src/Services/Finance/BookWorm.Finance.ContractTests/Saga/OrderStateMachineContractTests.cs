using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Finance.Saga;
using Microsoft.Extensions.Logging;

namespace BookWorm.Finance.ContractTests.Saga;

public sealed class OrderStateMachineContractTests
{
    private const string TestFullName = "John Doe";
    private const string TestEmail = "john.doe@example.com";
    private const decimal TestTotalMoney = 99.99m;
    private ILogger<OrderSaga> _logger = null!;
    private OrderStateMachineSettings _settings = null!;

    [Before(Test)]
    public void SetUp()
    {
        _settings = new() { MaxAttempts = 3, MaxRetryTimeout = TimeSpan.FromMinutes(30) };
        _logger = Mock.Of<ILogger<OrderSaga>>();
    }

    private UserCheckedOutIntegrationEvent CreateCheckedOutEvent(Guid orderId, Guid basketId)
    {
        return new(orderId, basketId, TestFullName, TestEmail, TestTotalMoney);
    }

    [Test]
    public async Task GivenUserCheckedOutEvent_WhenPublished_ThenSagaShouldStartAndQueueOutgoingMessages()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = CreateCheckedOutEvent(orderId, basketId);

        // Act
        var (saga, messages) = OrderSaga.Start(@event, _settings, _logger);

        // Assert
        await SnapshotTestHelper.VerifyCloudEvents(messages);
    }

    [Test]
    public async Task GivenBasketDeletedCompleteEvent_WhenHandled_ThenSagaShouldReturnDeleteBasketCompleteCommand()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var (saga, _) = OrderSaga.Start(
            CreateCheckedOutEvent(orderId, basketId),
            _settings,
            _logger
        );

        var @event = new BasketDeletedCompleteIntegrationEvent(orderId, basketId, TestTotalMoney);

        // Act
        var command = saga.Handle(@event);

        // Assert
        await SnapshotTestHelper.VerifyCloudEvent(command);
    }

    [Test]
    public async Task GivenBasketDeletedFailedEvent_WhenHandled_ThenSagaShouldTransitionToFailedStateAndPublishFailedCommand()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var (saga, _) = OrderSaga.Start(
            CreateCheckedOutEvent(orderId, basketId),
            _settings,
            _logger
        );

        var @event = new BasketDeletedFailedIntegrationEvent(
            orderId,
            basketId,
            TestEmail,
            TestTotalMoney
        );

        // Act
        var messages = saga.Handle(@event, _logger);

        // Assert
        saga.CurrentState.ShouldBe(OrderSagaStatus.BasketDeletionFailed);
        await SnapshotTestHelper.VerifyCloudEvents(messages);
    }

    [Test]
    public async Task GivenOrderCompletedEvent_WhenHandled_ThenSagaShouldTransitionToCompletedStateAndQueueNotification()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var (saga, _) = OrderSaga.Start(
            CreateCheckedOutEvent(orderId, basketId),
            _settings,
            _logger
        );

        var @event = new OrderStatusChangedToCompleteIntegrationEvent(
            orderId,
            basketId,
            TestFullName,
            TestEmail,
            TestTotalMoney
        );

        // Act
        var messages = saga.Handle(@event, _logger);

        // Assert
        saga.CurrentState.ShouldBe(OrderSagaStatus.Completed);
        await SnapshotTestHelper.VerifyCloudEvents(messages);
    }

    [Test]
    public async Task GivenOrderCancelledEvent_WhenHandled_ThenSagaShouldTransitionToCancelledStateAndQueueNotification()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var (saga, _) = OrderSaga.Start(
            CreateCheckedOutEvent(orderId, basketId),
            _settings,
            _logger
        );

        var @event = new OrderStatusChangedToCancelIntegrationEvent(
            orderId,
            basketId,
            TestFullName,
            TestEmail,
            TestTotalMoney
        );

        // Act
        var messages = saga.Handle(@event, _logger);

        // Assert
        saga.CurrentState.ShouldBe(OrderSagaStatus.Cancelled);
        await SnapshotTestHelper.VerifyCloudEvents(messages);
    }
}
