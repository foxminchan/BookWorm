using BookWorm.Contracts;
using BookWorm.Finance.Saga;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookWorm.Finance.UnitTests;

public sealed class OrderStateTests
{
    private static readonly OrderStateMachineSettings _settings = new()
    {
        MaxAttempts = 3,
        MaxRetryTimeout = TimeSpan.FromMinutes(10),
    };

    private static OrderSaga CreatePlacedSaga(
        string? email = "user@example.com",
        string? fullName = "John Doe"
    )
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            fullName!,
            email!,
            99.99m
        );

        return OrderSaga.Start(@event, _settings, NullLogger<OrderSaga>.Instance).Item1;
    }

    [Test]
    public void GivenNewSaga_WhenCreated_ThenShouldHaveDefaultValues()
    {
        var saga = new OrderSaga();

        saga.TimeoutRetryCount.ShouldBe(0);
        saga.CurrentState.ShouldBe(default);
    }

    [Test]
    public void GivenUserCheckedOutEvent_WhenStarting_ThenShouldMapAllFields()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            "Jane Doe",
            "jane@example.com",
            150.50m
        );

        var (saga, _) = OrderSaga.Start(@event, _settings, NullLogger<OrderSaga>.Instance);

        saga.OrderId.ShouldBe(orderId);
        saga.BasketId.ShouldBe(basketId);
        saga.FullName.ShouldBe("Jane Doe");
        saga.Email.ShouldBe("jane@example.com");
        saga.TotalMoney.ShouldBe(150.50m);
        saga.CurrentState.ShouldBe(OrderSagaStatus.Placed);
    }

    [Test]
    public void GivenBasketDeletedCompleteEvent_WhenHandling_ThenShouldUpdateFields()
    {
        var saga = CreatePlacedSaga();
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new BasketDeletedCompleteIntegrationEvent(orderId, basketId, 75.25m);

        saga.Handle(@event);

        saga.OrderId.ShouldBe(orderId);
        saga.BasketId.ShouldBe(basketId);
        saga.TotalMoney.ShouldBe(75.25m);
    }

    [Test]
    public void GivenBasketDeletedFailedEvent_WhenHandling_ThenShouldUpdateFields()
    {
        var saga = CreatePlacedSaga();
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new BasketDeletedFailedIntegrationEvent(
            orderId,
            basketId,
            "user@example.com",
            200.00m
        );

        saga.Handle(@event, NullLogger<OrderSaga>.Instance);

        saga.OrderId.ShouldBe(orderId);
        saga.BasketId.ShouldBe(basketId);
        saga.Email.ShouldBe("user@example.com");
        saga.TotalMoney.ShouldBe(200.00m);
    }

    [Test]
    public void GivenOrderCompletedEvent_WhenHandling_ThenShouldUpdateFields()
    {
        var saga = CreatePlacedSaga();
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new OrderStatusChangedToCompleteIntegrationEvent(
            orderId,
            basketId,
            "Bob",
            "bob@example.com",
            300.00m
        );

        saga.Handle(@event, NullLogger<OrderSaga>.Instance);

        saga.OrderId.ShouldBe(orderId);
        saga.BasketId.ShouldBe(basketId);
        saga.FullName.ShouldBe("Bob");
        saga.Email.ShouldBe("bob@example.com");
        saga.TotalMoney.ShouldBe(300.00m);
        saga.CurrentState.ShouldBe(OrderSagaStatus.Completed);
    }

    [Test]
    public void GivenOrderCancelledEvent_WhenHandling_ThenShouldUpdateFields()
    {
        var saga = CreatePlacedSaga();
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new OrderStatusChangedToCancelIntegrationEvent(
            orderId,
            basketId,
            "Alice",
            "alice@example.com",
            50.00m
        );

        saga.Handle(@event, NullLogger<OrderSaga>.Instance);

        saga.OrderId.ShouldBe(orderId);
        saga.BasketId.ShouldBe(basketId);
        saga.FullName.ShouldBe("Alice");
        saga.Email.ShouldBe("alice@example.com");
        saga.TotalMoney.ShouldBe(50.00m);
        saga.CurrentState.ShouldBe(OrderSagaStatus.Cancelled);
    }
}
