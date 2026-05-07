using BookWorm.Contracts;
using BookWorm.Finance.Saga;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookWorm.Finance.UnitTests;

public sealed class ActivityTests
{
    [Test]
    public void GivenUserCheckedOutEvent_WhenStartingSaga_ThenShouldMapStateAndEmitInitialMessages()
    {
        var settings = new OrderStateMachineSettings
        {
            MaxAttempts = 3,
            MaxRetryTimeout = TimeSpan.FromMinutes(10),
        };

        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            "John Doe",
            "john@example.com",
            99.99m
        );

        var (saga, messages) = OrderSaga.Start(@event, settings, NullLogger<OrderSaga>.Instance);

        saga.Id.ShouldBe(orderId);
        saga.OrderId.ShouldBe(orderId);
        saga.BasketId.ShouldBe(basketId);
        saga.FullName.ShouldBe("John Doe");
        saga.Email.ShouldBe("john@example.com");
        saga.TotalMoney.ShouldBe(99.99m);
        saga.CurrentState.ShouldBe(OrderSagaStatus.Placed);
        saga.OrderPlacedDate.ShouldNotBeNull();

        var placeOrder = messages.OfType<PlaceOrderCommand>().Single();
        placeOrder.OrderId.ShouldBe(orderId);
        placeOrder.BasketId.ShouldBe(basketId);
        placeOrder.FullName.ShouldBe("John Doe");
        placeOrder.Email.ShouldBe("john@example.com");
        placeOrder.TotalMoney.ShouldBe(99.99m);

        var timeout = messages.OfType<PlaceOrderTimeout>().Single();
        timeout.OrderId.ShouldBe(orderId);
        timeout.Delay.ShouldBe(settings.MaxRetryTimeout);
    }

    [Test]
    public void GivenBasketDeletedCompleteEvent_WhenHandling_ThenShouldMapStateAndReturnDeleteBasketCompleteCommand()
    {
        var saga = StartPlacedSaga();
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var totalMoney = 75.25m;

        var command = saga.Handle(new(orderId, basketId, totalMoney));

        saga.OrderId.ShouldBe(orderId);
        saga.BasketId.ShouldBe(basketId);
        saga.TotalMoney.ShouldBe(totalMoney);
        command.OrderId.ShouldBe(orderId);
        command.TotalMoney.ShouldBe(totalMoney);
    }

    [Test]
    public void GivenBasketDeletedFailedEvent_WhenHandling_ThenShouldMapStateAndReturnDeleteBasketFailedCommand()
    {
        var saga = StartPlacedSaga();
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var email = "failed@example.com";
        var totalMoney = 120.00m;

        var messages = saga.Handle(
            new BasketDeletedFailedIntegrationEvent(orderId, basketId, email, totalMoney),
            NullLogger<OrderSaga>.Instance
        );

        saga.OrderId.ShouldBe(orderId);
        saga.BasketId.ShouldBe(basketId);
        saga.Email.ShouldBe(email);
        saga.TotalMoney.ShouldBe(totalMoney);
        saga.CurrentState.ShouldBe(OrderSagaStatus.BasketDeletionFailed);

        var command = messages.OfType<DeleteBasketFailedCommand>().Single();
        command.OrderId.ShouldBe(orderId);
        command.BasketId.ShouldBe(basketId);
        command.Email.ShouldBe(email);
        command.TotalMoney.ShouldBe(totalMoney);
    }

    [Test]
    public void GivenOrderCompletedEvent_WhenHandling_ThenShouldMapStateAndReturnCompleteOrderCommand()
    {
        var saga = StartPlacedSaga();
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var fullName = "Jane Doe";
        var email = "jane@example.com";
        var totalMoney = 210.00m;

        var messages = saga.Handle(
            new OrderStatusChangedToCompleteIntegrationEvent(
                orderId,
                basketId,
                fullName,
                email,
                totalMoney
            ),
            NullLogger<OrderSaga>.Instance
        );

        saga.OrderId.ShouldBe(orderId);
        saga.BasketId.ShouldBe(basketId);
        saga.FullName.ShouldBe(fullName);
        saga.Email.ShouldBe(email);
        saga.TotalMoney.ShouldBe(totalMoney);
        saga.CurrentState.ShouldBe(OrderSagaStatus.Completed);

        var command = messages.OfType<CompleteOrderCommand>().Single();
        command.OrderId.ShouldBe(orderId);
        command.FullName.ShouldBe(fullName);
        command.Email.ShouldBe(email);
        command.TotalMoney.ShouldBe(totalMoney);
    }

    [Test]
    public void GivenOrderCancelledEvent_WhenHandling_ThenShouldMapStateAndReturnCancelOrderCommand()
    {
        var saga = StartPlacedSaga();
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var fullName = "Bob Doe";
        var email = "bob@example.com";
        var totalMoney = 180.00m;

        var messages = saga.Handle(
            new OrderStatusChangedToCancelIntegrationEvent(
                orderId,
                basketId,
                fullName,
                email,
                totalMoney
            ),
            NullLogger<OrderSaga>.Instance
        );

        saga.OrderId.ShouldBe(orderId);
        saga.BasketId.ShouldBe(basketId);
        saga.FullName.ShouldBe(fullName);
        saga.Email.ShouldBe(email);
        saga.TotalMoney.ShouldBe(totalMoney);
        saga.CurrentState.ShouldBe(OrderSagaStatus.Cancelled);

        var command = messages.OfType<CancelOrderCommand>().Single();
        command.OrderId.ShouldBe(orderId);
        command.FullName.ShouldBe(fullName);
        command.Email.ShouldBe(email);
        command.TotalMoney.ShouldBe(totalMoney);
    }

    private static OrderSaga StartPlacedSaga()
    {
        var settings = new OrderStateMachineSettings
        {
            MaxAttempts = 3,
            MaxRetryTimeout = TimeSpan.FromMinutes(10),
        };

        var @event = new UserCheckedOutIntegrationEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Starter",
            "starter@example.com",
            50.00m
        );

        return OrderSaga.Start(@event, settings, NullLogger<OrderSaga>.Instance).Item1;
    }
}
