using BookWorm.Contracts;
using BookWorm.Finance.Saga;

namespace BookWorm.Finance.UnitTests;

public sealed class OrderStateTests
{
    [Test]
    [Arguments("user@example.com", "John Doe", true)]
    [Arguments("user@example.com", "", false)]
    [Arguments("user@example.com", "   ", false)]
    [Arguments("user@example.com", null, false)]
    [Arguments("", "John Doe", false)]
    [Arguments("   ", "John Doe", false)]
    [Arguments(null, "John Doe", false)]
    [Arguments(null, null, false)]
    public void GivenEmailAndFullName_WhenCheckingCanSendNotification_ThenShouldReflectPresence(
        string? email,
        string? fullName,
        bool expected
    )
    {
        var state = new OrderState { Email = email, FullName = fullName };

        state.CanSendNotification.ShouldBe(expected);
    }

    [Test]
    [Arguments(99.99, 99.99)]
    [Arguments(0.0, 0.0)]
    [Arguments(null, 0.0)]
    public void GivenTotalMoney_WhenGettingEffectiveValue_ThenShouldReturnValueOrZero(
        double? totalMoney,
        double expected
    )
    {
        var state = new OrderState
        {
            TotalMoney = totalMoney is not null ? (decimal)totalMoney : null,
        };

        state.EffectiveTotalMoney.ShouldBe((decimal)expected);
    }

    [Test]
    public void GivenOrderState_WhenIncrementingRetry_ThenShouldIncreaseTimeoutRetryCount()
    {
        var state = new OrderState();

        state.TimeoutRetryCount.ShouldBe(0);

        state.IncrementRetry();
        state.TimeoutRetryCount.ShouldBe(1);

        state.IncrementRetry();
        state.TimeoutRetryCount.ShouldBe(2);
    }

    [Test]
    [Arguments(0, 3, false)]
    [Arguments(2, 3, false)]
    [Arguments(3, 3, true)]
    [Arguments(4, 3, true)]
    [Arguments(0, 0, true)]
    public void GivenRetryCount_WhenCheckingMaxRetries_ThenShouldCompareToMaxAttempts(
        int retryCount,
        int maxAttempts,
        bool expected
    )
    {
        var state = new OrderState { TimeoutRetryCount = retryCount };

        state.HasExceededMaxRetries(maxAttempts).ShouldBe(expected);
    }

    [Test]
    public void GivenUserCheckedOutEvent_WhenMappingToState_ThenShouldMapAllFields()
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

        var state = new OrderState();
        state.MapFrom(@event);

        state.OrderId.ShouldBe(orderId);
        state.BasketId.ShouldBe(basketId);
        state.FullName.ShouldBe("Jane Doe");
        state.Email.ShouldBe("jane@example.com");
        state.TotalMoney.ShouldBe(150.50m);
    }

    [Test]
    public void GivenBasketDeletedFailedEvent_WhenMappingToState_ThenShouldMapRelevantFields()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new BasketDeletedFailedIntegrationEvent(
            orderId,
            basketId,
            "user@example.com",
            200.00m
        );

        var state = new OrderState();
        state.MapFrom(@event);

        state.OrderId.ShouldBe(orderId);
        state.BasketId.ShouldBe(basketId);
        state.Email.ShouldBe("user@example.com");
        state.TotalMoney.ShouldBe(200.00m);
    }

    [Test]
    public void GivenBasketDeletedCompleteEvent_WhenMappingToState_ThenShouldMapRelevantFields()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new BasketDeletedCompleteIntegrationEvent(orderId, basketId, 75.25m);

        var state = new OrderState();
        state.MapFrom(@event);

        state.OrderId.ShouldBe(orderId);
        state.BasketId.ShouldBe(basketId);
        state.TotalMoney.ShouldBe(75.25m);
    }

    [Test]
    public void GivenOrderCompletedEvent_WhenMappingToState_ThenShouldMapAllFields()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new OrderStatusChangedToCompleteIntegrationEvent(
            orderId,
            basketId,
            "Bob",
            "bob@example.com",
            300.00m
        );

        var state = new OrderState();
        state.MapFrom(@event);

        state.OrderId.ShouldBe(orderId);
        state.BasketId.ShouldBe(basketId);
        state.FullName.ShouldBe("Bob");
        state.Email.ShouldBe("bob@example.com");
        state.TotalMoney.ShouldBe(300.00m);
    }

    [Test]
    public void GivenOrderCancelledEvent_WhenMappingToState_ThenShouldMapAllFields()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var @event = new OrderStatusChangedToCancelIntegrationEvent(
            orderId,
            basketId,
            "Alice",
            "alice@example.com",
            50.00m
        );

        var state = new OrderState();
        state.MapFrom(@event);

        state.OrderId.ShouldBe(orderId);
        state.BasketId.ShouldBe(basketId);
        state.FullName.ShouldBe("Alice");
        state.Email.ShouldBe("alice@example.com");
        state.TotalMoney.ShouldBe(50.00m);
    }
}
