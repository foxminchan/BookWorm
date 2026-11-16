using BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Finance.Domain.Events;
using BookWorm.Finance.Domain.Exceptions;

namespace BookWorm.Finance.UnitTests.Domain;

public sealed class OrderAggregateTests
{
    private static readonly DateTime _fixedUtcNow = new(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public void GivenValidParameters_WhenPlacingOrder_ThenShouldInitializeAndRaiseOrderPlacedEvent()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();

        var order = Order.Place(
            orderId,
            basketId,
            new("John Doe", "john@example.com"),
            Money.Of(99.99m),
            3,
            _fixedUtcNow
        );

        order.Id.ShouldBe(orderId);
        order.BasketId.ShouldBe(basketId);
        order.Customer.FullName.ShouldBe("John Doe");
        order.Customer.Email.ShouldBe("john@example.com");
        order.TotalMoney.Amount.ShouldBe(99.99m);
        order.Status.ShouldBe(OrderStatus.Placed);
        order.MaxAttempts.ShouldBe(3);
        order.TimeoutRetryCount.ShouldBe(0);
        order.BasketDeleted.ShouldBeFalse();
        order.CanSendNotification.ShouldBeTrue();
        order.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<OrderPlacedEvent>();
    }

    [Test]
    public void GivenInvalidParameters_WhenPlacingOrder_ThenShouldThrowFinanceDomainException()
    {
        var validId = Guid.CreateVersion7();

        Should.Throw<FinanceDomainException>(() =>
            Order.Place(Guid.Empty, validId, new("John", "j@e.com"), Money.Of(10m), 3, _fixedUtcNow)
        );
        Should.Throw<FinanceDomainException>(() =>
            Order.Place(validId, Guid.Empty, new("John", "j@e.com"), Money.Of(10m), 3, _fixedUtcNow)
        );
        Should.Throw<FinanceDomainException>(() =>
            Order.Place(validId, validId, new("John", "j@e.com"), Money.Of(10m), 0, _fixedUtcNow)
        );
        Should.Throw<FinanceDomainException>(() => Money.Of(-1m));
    }

    [Test]
    public void GivenPlacedOrder_WhenMarkingBasketDeleted_ThenShouldUpdateStateAndRaiseEvent()
    {
        var order = CreatePlacedOrder();
        var newBasketId = Guid.CreateVersion7();

        order.MarkBasketDeleted(newBasketId, Money.Of(55.50m));

        order.BasketDeleted.ShouldBeTrue();
        order.BasketId.ShouldBe(newBasketId);
        order.TotalMoney.Amount.ShouldBe(55.50m);
        order.DomainEvents.OfType<BasketDeletedEvent>().ShouldHaveSingleItem();
    }

    [Test]
    public void GivenPlacedOrder_WhenFailingBasketDeletion_ThenShouldBecomeTerminalAndRaiseEvent()
    {
        var order = CreatePlacedOrder();

        order.FailBasketDeletion(order.BasketId, "failed@example.com", Money.Of(40m));

        order.Status.ShouldBe(OrderStatus.BasketDeletionFailed);
        order.IsTerminal.ShouldBeTrue();
        order.Customer.Email.ShouldBe("failed@example.com");
        order.DomainEvents.OfType<BasketDeletionFailedEvent>().ShouldHaveSingleItem();

        Should.Throw<FinanceDomainException>(() =>
            order.Complete(order.BasketId, new("John", "john@example.com"), Money.Of(40m))
        );
    }

    [Test]
    public void GivenPlacedOrder_WhenRetryingAndExceedingMaxAttempts_ThenShouldAllowCancelDueToMaxRetries()
    {
        var order = CreatePlacedOrder(2);

        order.RecordTimeoutRetry().ShouldBeTrue();
        order.TimeoutRetryCount.ShouldBe(1);
        order.HasExceededMaxRetries.ShouldBeFalse();

        order.RecordTimeoutRetry().ShouldBeFalse();
        order.TimeoutRetryCount.ShouldBe(2);
        order.HasExceededMaxRetries.ShouldBeTrue();

        order.CancelDueToMaxRetries();

        order.Status.ShouldBe(OrderStatus.Cancelled);
        order.IsTerminal.ShouldBeTrue();
        order.DomainEvents.OfType<OrderTimeoutRetriedEvent>().Count().ShouldBe(2);
        order.DomainEvents.OfType<OrderCancelledEvent>().ShouldHaveSingleItem();
    }

    [Test]
    public void GivenPlacedOrder_WhenCancellingBeforeMaxRetries_ThenShouldThrowFinanceDomainException()
    {
        var order = CreatePlacedOrder(3);
        order.RecordTimeoutRetry().ShouldBeTrue();

        Should.Throw<FinanceDomainException>(order.CancelDueToMaxRetries);
    }

    [Test]
    public void GivenValueObjects_WhenComparingEqualValues_ThenShouldHaveValueEquality()
    {
        Money.Of(10m).ShouldBe(Money.Of(10m));
        new CustomerContact("John Doe", "john@example.com").ShouldBe(
            new("John Doe", "john@example.com")
        );
        new CustomerContact("  ", null).CanNotify.ShouldBeFalse();
    }

    private static Order CreatePlacedOrder(int maxAttempts = 3)
    {
        return Order.Place(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            new("John Doe", "john@example.com"),
            Money.Of(99.99m),
            maxAttempts,
            _fixedUtcNow
        );
    }
}
