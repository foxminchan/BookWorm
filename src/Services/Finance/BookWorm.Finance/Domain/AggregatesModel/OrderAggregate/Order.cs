using BookWorm.Finance.Domain.Events;
using BookWorm.Finance.Domain.Exceptions;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;

public sealed class Order : Entity, IAggregateRoot
{
    private Order(
        Guid id,
        Guid basketId,
        CustomerContact customer,
        Money totalMoney,
        int maxAttempts,
        DateTime orderPlacedDate
    )
    {
        if (id == Guid.Empty)
        {
            throw new FinanceDomainException("OrderId cannot be empty.");
        }

        if (basketId == Guid.Empty)
        {
            throw new FinanceDomainException("BasketId cannot be empty.");
        }

        if (maxAttempts <= 0)
        {
            throw new FinanceDomainException("MaxAttempts must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(totalMoney);

        Id = id;
        BasketId = basketId;
        Customer = customer;
        TotalMoney = totalMoney;
        MaxAttempts = maxAttempts;
        OrderPlacedDate = orderPlacedDate;
        Status = OrderStatus.Placed;

        RegisterDomainEvent(new OrderPlacedEvent(this));
    }

    public Guid BasketId { get; private set; }
    public CustomerContact Customer { get; private set; }
    public Money TotalMoney { get; private set; }
    public DateTime OrderPlacedDate { get; }
    public OrderStatus Status { get; private set; }
    public int TimeoutRetryCount { get; private set; }
    public int MaxAttempts { get; }
    public bool BasketDeleted { get; private set; }

    public bool CanSendNotification => Customer.CanNotify;
    public bool HasExceededMaxRetries => TimeoutRetryCount >= MaxAttempts;

    public bool IsTerminal =>
        Status
            is OrderStatus.Completed
                or OrderStatus.Cancelled
                or OrderStatus.BasketDeletionFailed;

    public static Order Place(
        Guid orderId,
        Guid basketId,
        CustomerContact customer,
        Money totalMoney,
        int maxAttempts,
        DateTime orderPlacedDate
    )
    {
        return new(orderId, basketId, customer, totalMoney, maxAttempts, orderPlacedDate);
    }

    /// <summary>
    ///     Marks the order as having its associated basket deleted, typically after a successful order placement.
    /// </summary>
    /// <param name="basketId">The ID of the basket associated with the order.</param>
    /// <param name="totalMoney">The total monetary value of the order.</param>
    public void MarkBasketDeleted(Guid basketId, Money totalMoney)
    {
        EnsureValidBasketId(basketId);
        ArgumentNullException.ThrowIfNull(totalMoney);

        ApplyTransition(
            OrderLifecycleAction.MarkBasketDeleted,
            static order => new BasketDeletedEvent(order)
        );

        BasketId = basketId;
        TotalMoney = totalMoney;
    }

    /// <summary>
    ///     Marks the order as failed due to a basket deletion failure, typically
    ///     when the basket service fails to delete the basket after a successful order placement.
    /// </summary>
    /// <param name="basketId">The ID of the basket associated with the order.</param>
    /// <param name="email">The email of the customer associated with the order.</param>
    /// <param name="totalMoney">The total monetary value of the order.</param>
    public void FailBasketDeletion(Guid basketId, string? email, Money totalMoney)
    {
        EnsureValidBasketId(basketId);
        ArgumentNullException.ThrowIfNull(totalMoney);

        ApplyTransition(
            OrderLifecycleAction.FailBasketDeletion,
            static order => new BasketDeletionFailedEvent(order)
        );

        BasketId = basketId;
        Customer = Customer.WithEmail(email);
        TotalMoney = totalMoney;
    }

    /// <summary>
    ///     Cancels the order, typically due to a timeout or other failure in the order processing lifecycle.
    /// </summary>
    /// <param name="basketId">The ID of the basket associated with the order.</param>
    /// <param name="customer">The customer contact information.</param>
    /// <param name="totalMoney">The total monetary value of the order.</param>
    public void Complete(Guid basketId, CustomerContact customer, Money totalMoney)
    {
        EnsureValidBasketId(basketId);
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(totalMoney);

        ApplyTransition(OrderLifecycleAction.Complete, static order => new OrderCompletedEvent(
            order
        ));

        BasketId = basketId;
        Customer = customer;
        TotalMoney = totalMoney;
    }

    /// <summary>
    ///     Cancels the order, typically due to a timeout or other failure in the order processing workflow.
    /// </summary>
    /// <param name="basketId">The ID of the basket associated with the order.</param>
    /// <param name="customer">The customer contact information.</param>
    /// <param name="totalMoney">The total monetary value of the order.</param>
    public void Cancel(Guid basketId, CustomerContact customer, Money totalMoney)
    {
        EnsureValidBasketId(basketId);
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(totalMoney);

        ApplyTransition(OrderLifecycleAction.Cancel, static order => new OrderCancelledEvent(
            order
        ));

        BasketId = basketId;
        Customer = customer;
        TotalMoney = totalMoney;
    }

    /// <summary>
    ///     Records a timeout retry attempt and returns whether another retry should be dispatched.
    /// </summary>
    public bool RecordTimeoutRetry()
    {
        ApplyTransition(
            OrderLifecycleAction.RecordTimeoutRetry,
            static order => new OrderTimeoutRetriedEvent(order)
        );

        return !HasExceededMaxRetries;
    }

    /// <summary>
    ///     Cancels the order due to exceeding the maximum number of allowed retries.
    /// </summary>
    public void CancelDueToMaxRetries()
    {
        ApplyTransition(
            OrderLifecycleAction.CancelDueToMaxRetries,
            static order => new OrderCancelledEvent(order)
        );
    }

    private void ApplyTransition(OrderLifecycleAction action, Func<Order, DomainEvent> createEvent)
    {
        var (nextState, _) = OrderLifecycleDecider.Instance.Handle(
            new(action),
            new(Status, BasketDeleted, TimeoutRetryCount, MaxAttempts)
        );

        Status = nextState.Status;
        BasketDeleted = nextState.BasketDeleted;
        TimeoutRetryCount = nextState.TimeoutRetryCount;
        RegisterDomainEvent(createEvent(this));
    }

    private static void EnsureValidBasketId(Guid basketId)
    {
        if (basketId == Guid.Empty)
        {
            throw new FinanceDomainException("BasketId cannot be empty.");
        }
    }
}
