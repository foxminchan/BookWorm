using BookWorm.Contracts;
using BookWorm.Finance.Saga;
using static BookWorm.Finance.UnitTests.Helpers.OrderSagaTestHelper;

namespace BookWorm.Finance.UnitTests;

public sealed class OrderSagaTests
{
    [Test]
    public async Task GivenUserCheckedOutEvent_WhenStarting_ThenShouldTransitionToPlacedAndPublishCommand()
    {
        // Arrange
        var testData = CreateTestOrderData();
        var published = new List<object>();
        var context = CreateMessageContextMock(published);
        var settings = CreateSettings();
        var saga = new OrderSaga();

        var @event = new UserCheckedOutIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            testData.FullName,
            testData.Email,
            testData.TotalMoney
        );

        // Act
        await saga.Start(@event, context.Object, settings);

        // Assert
        saga.Id.ShouldBe(testData.OrderId);
        saga.CurrentState.ShouldBe("Placed");
        AssertSagaState(saga, testData);

        var command = published.OfType<PlaceOrderCommand>().ShouldHaveSingleItem();
        command.OrderId.ShouldBe(testData.OrderId);
        command.BasketId.ShouldBe(testData.BasketId);
        command.FullName.ShouldBe(testData.FullName);
        command.Email.ShouldBe(testData.Email);
        command.TotalMoney.ShouldBe(testData.TotalMoney);

        published.OfType<PlaceOrderTimeoutIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Test]
    public async Task GivenBasketDeletedComplete_WhenInPlacedState_ThenShouldPublishDeleteBasketCompleteCommand()
    {
        // Arrange
        var testData = CreateTestOrderData();
        var saga = await CreatePlacedSaga(testData);
        var published = new List<object>();
        var context = CreateMessageContextMock(published);

        var @event = new BasketDeletedCompleteIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            testData.TotalMoney
        );

        // Act
        await saga.Handle(@event, context.Object);

        // Assert
        saga.CurrentState.ShouldBe("Placed");
        saga.IsTimeoutCancelled.ShouldBeTrue();
        saga.IsCompleted().ShouldBeFalse();

        var command = published.OfType<DeleteBasketCompleteCommand>().ShouldHaveSingleItem();
        command.OrderId.ShouldBe(testData.OrderId);
        command.TotalMoney.ShouldBe(testData.TotalMoney);
    }

    [Test]
    public async Task GivenBasketDeletedFailed_WhenInPlacedState_ThenShouldTransitionToFailedAndPublishCommand()
    {
        // Arrange
        var testData = CreateTestOrderData();
        var saga = await CreatePlacedSaga(testData);
        var published = new List<object>();
        var context = CreateMessageContextMock(published);

        var @event = new BasketDeletedFailedIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            testData.Email,
            testData.TotalMoney
        );

        // Act
        await saga.Handle(@event, context.Object);

        // Assert
        saga.CurrentState.ShouldBe("Failed");
        saga.IsTimeoutCancelled.ShouldBeTrue();
        saga.IsCompleted().ShouldBeFalse();

        var command = published.OfType<DeleteBasketFailedCommand>().ShouldHaveSingleItem();
        command.OrderId.ShouldBe(testData.OrderId);
        command.BasketId.ShouldBe(testData.BasketId);
        command.Email.ShouldBe(testData.Email);
        command.TotalMoney.ShouldBe(testData.TotalMoney);
    }

    [Test]
    [Arguments(DefaultTestEmail)]
    [Arguments(null)]
    public async Task GivenOrderCompleted_WhenInPlacedState_ThenShouldPublishCompleteOrderCommandOnlyWithValidEmail(
        string? email
    )
    {
        // Arrange
        var testData = CreateTestOrderData(email: email);
        var saga = await CreatePlacedSaga(testData);
        var published = new List<object>();
        var context = CreateMessageContextMock(published);

        var @event = new OrderStatusChangedToCompleteIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            testData.FullName,
            email,
            testData.TotalMoney
        );

        // Act
        await saga.Handle(@event, context.Object);

        // Assert
        saga.CurrentState.ShouldBe("Completed");
        saga.IsCompleted().ShouldBeTrue();

        if (email is not null)
        {
            var command = published.OfType<CompleteOrderCommand>().ShouldHaveSingleItem();
            command.OrderId.ShouldBe(testData.OrderId);
            command.FullName.ShouldBe(testData.FullName);
            command.Email.ShouldBe(email);
            command.TotalMoney.ShouldBe(testData.TotalMoney);
        }
        else
        {
            published.OfType<CompleteOrderCommand>().ShouldBeEmpty();
        }
    }

    [Test]
    [Arguments(DefaultTestEmail)]
    [Arguments(null)]
    public async Task GivenOrderCancelled_WhenInPlacedState_ThenShouldPublishCancelOrderCommandOnlyWithValidEmail(
        string? email
    )
    {
        // Arrange
        var testData = CreateTestOrderData(email: email);
        var saga = await CreatePlacedSaga(testData);
        var published = new List<object>();
        var context = CreateMessageContextMock(published);

        var @event = new OrderStatusChangedToCancelIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            testData.FullName,
            email,
            testData.TotalMoney
        );

        // Act
        await saga.Handle(@event, context.Object);

        // Assert
        saga.CurrentState.ShouldBe("Cancelled");
        saga.IsCompleted().ShouldBeTrue();

        if (email is not null)
        {
            var command = published.OfType<CancelOrderCommand>().ShouldHaveSingleItem();
            command.OrderId.ShouldBe(testData.OrderId);
            command.FullName.ShouldBe(testData.FullName);
            command.Email.ShouldBe(email);
            command.TotalMoney.ShouldBe(testData.TotalMoney);
        }
        else
        {
            published.OfType<CancelOrderCommand>().ShouldBeEmpty();
        }
    }

    [Test]
    public async Task GivenTimeout_WhenRetryCountLessThanMax_ThenShouldRetry()
    {
        // Arrange
        var testData = CreateTestOrderData();
        var saga = await CreatePlacedSaga(testData);
        var published = new List<object>();
        var context = CreateMessageContextMock(published);
        var settings = CreateSettings(maxAttempts: 3);

        // Act
        await saga.Handle(
            new PlaceOrderTimeoutIntegrationEvent(testData.OrderId),
            context.Object,
            settings
        );

        // Assert
        saga.CurrentState.ShouldBe("Placed");
        saga.TimeoutRetryCount.ShouldBe(1);
        saga.IsCompleted().ShouldBeFalse();

        published.OfType<PlaceOrderCommand>().ShouldHaveSingleItem();
        published.OfType<PlaceOrderTimeoutIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Test]
    public async Task GivenTimeout_WhenRetryCountReachesMax_ThenShouldTransitionToFailed()
    {
        // Arrange
        var testData = CreateTestOrderData();
        var saga = await CreatePlacedSaga(testData);
        saga.TimeoutRetryCount = 2; // Already retried twice
        var published = new List<object>();
        var context = CreateMessageContextMock(published);
        var settings = CreateSettings(maxAttempts: 3);

        // Act
        await saga.Handle(
            new PlaceOrderTimeoutIntegrationEvent(testData.OrderId),
            context.Object,
            settings
        );

        // Assert
        saga.CurrentState.ShouldBe("Failed");
        saga.TimeoutRetryCount.ShouldBe(3);
        saga.IsCompleted().ShouldBeTrue();

        var command = published.OfType<CancelOrderCommand>().ShouldHaveSingleItem();
        command.OrderId.ShouldBe(testData.OrderId);
    }

    [Test]
    public async Task GivenTimeout_WhenTimeoutCancelled_ThenShouldIgnore()
    {
        // Arrange
        var testData = CreateTestOrderData();
        var saga = await CreatePlacedSaga(testData);
        saga.IsTimeoutCancelled = true;
        var published = new List<object>();
        var context = CreateMessageContextMock(published);
        var settings = CreateSettings();

        // Act
        await saga.Handle(
            new PlaceOrderTimeoutIntegrationEvent(testData.OrderId),
            context.Object,
            settings
        );

        // Assert
        saga.CurrentState.ShouldBe("Placed");
        saga.TimeoutRetryCount.ShouldBe(0);
        published.ShouldBeEmpty();
    }

    [Test]
    public async Task GivenEventInWrongState_WhenHandling_ThenShouldIgnore()
    {
        // Arrange — saga is in "Failed" state
        var testData = CreateTestOrderData();
        var saga = await CreatePlacedSaga(testData);
        saga.CurrentState = "Failed";
        var published = new List<object>();
        var context = CreateMessageContextMock(published);

        // Act — try to handle BasketDeletedComplete in Failed state
        await saga.Handle(
            new BasketDeletedCompleteIntegrationEvent(
                testData.OrderId,
                testData.BasketId,
                testData.TotalMoney
            ),
            context.Object
        );

        // Assert — nothing happened
        saga.CurrentState.ShouldBe("Failed");
        published.ShouldBeEmpty();
    }

    private static async Task<OrderSaga> CreatePlacedSaga(TestOrderData testData)
    {
        var saga = new OrderSaga();
        var context = CreateMessageContextMock();
        var settings = CreateSettings();

        await saga.Start(
            new UserCheckedOutIntegrationEvent(
                testData.OrderId,
                testData.BasketId,
                testData.FullName,
                testData.Email,
                testData.TotalMoney
            ),
            context.Object,
            settings
        );

        return saga;
    }
}
