using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Finance.Saga;
using Microsoft.Extensions.Options;
using Wolverine;

namespace BookWorm.Finance.ContractTests.Saga;

public sealed class OrderSagaContractTests
{
    private const string TestFullName = "John Doe";
    private const string TestEmail = "john.doe@example.com";
    private const decimal TestTotalMoney = 99.99m;
    private Mock<IMessageContext> _contextMock = null!;
    private List<object> _published = null!;
    private IOptions<OrderStateMachineSettings> _options = null!;

    [Before(Test)]
    public void SetUp()
    {
        _published = [];
        _contextMock = new();
        _contextMock
            .Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<DeliveryOptions?>()))
            .Callback<object, DeliveryOptions?>((msg, _) => _published.Add(msg))
            .Returns(ValueTask.CompletedTask);
        _contextMock
            .Setup(x => x.SendAsync(It.IsAny<object>(), It.IsAny<DeliveryOptions?>()))
            .Callback<object, DeliveryOptions?>((msg, _) => _published.Add(msg))
            .Returns(ValueTask.CompletedTask);

        _options = Options.Create(
            new OrderStateMachineSettings
            {
                MaxAttempts = 3,
                MaxRetryTimeout = TimeSpan.FromMinutes(30),
            }
        );
    }

    private async Task<(OrderSaga Saga, Guid OrderId, Guid BasketId)> InitializeSagaAsync()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var saga = new OrderSaga();
        var initialEvent = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            TestFullName,
            TestEmail,
            TestTotalMoney
        );
        await saga.Start(initialEvent, _contextMock.Object, _options);
        _published.Clear();
        return (saga, orderId, basketId);
    }

    [Test]
    public async Task GivenUserCheckedOutEvent_WhenPublished_ThenSagaShouldTransitionToPlacedState()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var saga = new OrderSaga();

        var @event = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            TestFullName,
            TestEmail,
            TestTotalMoney
        );

        // Act
        await saga.Start(@event, _contextMock.Object, _options);

        // Assert
        await SnapshotTestHelper.Verify(
            new
            {
                saga,
                @event,
                published = _published,
            }
        );
    }

    [Test]
    public async Task GivenBasketDeletedCompleteEvent_WhenPublished_ThenSagaShouldPublishDeleteBasketCompleteCommand()
    {
        // Arrange - Initialize saga to Placed state
        var (saga, orderId, basketId) = await InitializeSagaAsync();

        var basketDeletedEvent = new BasketDeletedCompleteIntegrationEvent(
            orderId,
            basketId,
            TestTotalMoney
        );

        // Act
        await saga.Handle(basketDeletedEvent, _contextMock.Object);

        // Assert
        await SnapshotTestHelper.Verify(
            new
            {
                saga,
                @event = basketDeletedEvent,
                published = _published,
            }
        );
    }

    [Test]
    public async Task GivenBasketDeletedFailedEvent_WhenPublished_ThenSagaShouldTransitionToFailedStateAndPublishFailedCommand()
    {
        // Arrange - Initialize saga to Placed state
        var (saga, orderId, basketId) = await InitializeSagaAsync();

        var basketDeletedFailedEvent = new BasketDeletedFailedIntegrationEvent(
            orderId,
            basketId,
            TestEmail,
            TestTotalMoney
        );

        // Act
        await saga.Handle(basketDeletedFailedEvent, _contextMock.Object);

        // Assert
        await SnapshotTestHelper.Verify(
            new
            {
                saga,
                @event = basketDeletedFailedEvent,
                published = _published,
            }
        );
    }

    [Test]
    public async Task GivenOrderCompletedEvent_WhenPublished_ThenSagaShouldTransitionToCompletedStateAndFinalize()
    {
        // Arrange - Initialize saga to Placed state
        var (saga, orderId, basketId) = await InitializeSagaAsync();

        var orderCompletedEvent = new OrderStatusChangedToCompleteIntegrationEvent(
            orderId,
            basketId,
            TestFullName,
            TestEmail,
            TestTotalMoney
        );

        // Act
        await saga.Handle(orderCompletedEvent, _contextMock.Object);

        // Assert
        await SnapshotTestHelper.Verify(
            new
            {
                saga,
                @event = orderCompletedEvent,
                published = _published,
            }
        );
    }

    [Test]
    public async Task GivenOrderCancelledEvent_WhenPublished_ThenSagaShouldTransitionToCancelledStateAndFinalize()
    {
        // Arrange - Initialize saga to Placed state
        var (saga, orderId, basketId) = await InitializeSagaAsync();

        var orderCancelledEvent = new OrderStatusChangedToCancelIntegrationEvent(
            orderId,
            basketId,
            TestFullName,
            TestEmail,
            TestTotalMoney
        );

        // Act
        await saga.Handle(orderCancelledEvent, _contextMock.Object);

        // Assert
        await SnapshotTestHelper.Verify(
            new
            {
                saga,
                @event = orderCancelledEvent,
                published = _published,
            }
        );
    }
}
