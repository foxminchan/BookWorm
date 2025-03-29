using BookWorm.Contracts;
using BookWorm.Finance.Saga;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Finance.UnitTests;

public sealed class OrderStateMachineTests
{
    private const string? DefaultTestEmail = "example@email.com";
    private ServiceProvider _provider = null!;
    private ITestHarness _harness = null!;
    private ISagaStateMachineTestHarness<OrderStateMachine, OrderState> _sagaHarness = null!;

    [Before(Test)]
    public async Task Setup()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
            )
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();

        _sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
    }

    [After(Test)]
    public async Task TearDown()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    private async Task InitializeSagaToPlacedState(
        Guid? orderId = null,
        Guid? basketId = null,
        string? email = DefaultTestEmail,
        decimal totalMoney = 100.0m
    )
    {
        orderId ??= Guid.CreateVersion7();
        basketId ??= Guid.CreateVersion7();

        var checkoutEvent = new UserCheckedOutIntegrationEvent(
            orderId.Value,
            basketId.Value,
            email,
            totalMoney
        );

        await _harness.Bus.Publish(checkoutEvent);

        // Wait for the saga to be created and transition to Placed state
        await _sagaHarness.Created.Any(x => x.CorrelationId == orderId);
    }

    private async Task AssertEventConsumed<TEvent>()
        where TEvent : class
    {
        (await _harness.Consumed.Any<TEvent>()).ShouldBeTrue();
        (await _sagaHarness.Consumed.Any<TEvent>()).ShouldBeTrue();
    }

    private OrderState AssertSagaInState(Guid orderId, State state)
    {
        var instance = _sagaHarness.Created.ContainsInState(
            orderId,
            _sagaHarness.StateMachine,
            state
        );

        instance.ShouldNotBeNull();
        instance.CurrentState.ShouldBe(state.Name);

        return instance;
    }

    [Test]
    public async Task GivenUserCheckedOutIntegrationEvent_WhenConsuming_ThenShouldCreateOrderStateAndPublishPlaceOrderCommand()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const decimal totalMoney = 100.0m;

        var @event = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            DefaultTestEmail,
            totalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await AssertEventConsumed<UserCheckedOutIntegrationEvent>();
        (await _sagaHarness.Created.Any(x => x.CorrelationId == orderId)).ShouldBeTrue();

        var instance = AssertSagaInState(orderId, _sagaHarness.StateMachine.Placed);

        instance.OrderId.ShouldBe(orderId);
        instance.BasketId.ShouldBe(basketId);
        instance.Email.ShouldBe(DefaultTestEmail);
        instance.TotalMoney.ShouldBe(totalMoney);
        instance.Version.ShouldBeGreaterThanOrEqualTo(0);
        instance.CurrentState.ShouldBe(_sagaHarness.StateMachine.Placed.Name);
        instance.OrderPlacedDate.ShouldNotBe(DateTime.MinValue);

        (await _harness.Published.Any<PlaceOrderCommand>()).ShouldBeTrue();

        var publishedMessage = _harness.Published.Select<PlaceOrderCommand>().First();
        var message = publishedMessage.Context.Message;
        message.BasketId.ShouldBe(basketId);
        message.OrderId.ShouldBe(orderId);
        message.Email.ShouldBe(DefaultTestEmail);
        message.TotalMoney.ShouldBe(totalMoney);
    }

    [Test]
    public async Task GivenBasketDeletedCompleteIntegrationEvent_WhenConsuming_ThenShouldPublishDeleteBasketCompleteCommand()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const decimal totalMoney = 100.0m;

        await InitializeSagaToPlacedState(orderId, basketId);

        var @event = new BasketDeletedCompleteIntegrationEvent(orderId, basketId, totalMoney);

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await AssertEventConsumed<BasketDeletedCompleteIntegrationEvent>();

        var instance = AssertSagaInState(orderId, _sagaHarness.StateMachine.Placed);

        instance.OrderId.ShouldBe(orderId);
        instance.BasketId.ShouldBe(basketId);
        instance.TotalMoney.ShouldBe(totalMoney);
        instance.Version.ShouldBeGreaterThanOrEqualTo(0);
        instance.CurrentState.ShouldBe(_sagaHarness.StateMachine.Placed.Name);
        instance.OrderPlacedDate.ShouldNotBe(DateTime.MinValue);

        (await _harness.Published.Any<DeleteBasketCompleteCommand>()).ShouldBeTrue();
        var publishedMessage = _harness.Published.Select<DeleteBasketCompleteCommand>().First();
        var message = publishedMessage.Context.Message;
        message.OrderId.ShouldBe(orderId);
        message.TotalMoney.ShouldBe(totalMoney);
    }

    [Test]
    public async Task GivenBasketDeletedFailedIntegrationEvent_WhenConsuming_ThenShouldPublishDeleteBasketFailedCommand()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const decimal totalMoney = 100.0m;

        await InitializeSagaToPlacedState(orderId, basketId);

        var @event = new BasketDeletedFailedIntegrationEvent(
            orderId,
            basketId,
            DefaultTestEmail,
            totalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await AssertEventConsumed<BasketDeletedFailedIntegrationEvent>();

        var instance = AssertSagaInState(orderId, _sagaHarness.StateMachine.Failed);

        instance.OrderId.ShouldBe(orderId);
        instance.BasketId.ShouldBe(basketId);
        instance.TotalMoney.ShouldBe(totalMoney);
        instance.Version.ShouldBeGreaterThanOrEqualTo(0);
        instance.CurrentState.ShouldBe(_sagaHarness.StateMachine.Failed.Name);
        instance.OrderPlacedDate.ShouldNotBe(DateTime.MinValue);

        (await _harness.Published.Any<DeleteBasketFailedCommand>()).ShouldBeTrue();
        var publishedMessage = _harness.Published.Select<DeleteBasketFailedCommand>().First();
        var message = publishedMessage.Context.Message;
        message.OrderId.ShouldBe(orderId);
        message.BasketId.ShouldBe(basketId);
        message.Email.ShouldBe(DefaultTestEmail);
        message.TotalMoney.ShouldBe(totalMoney);
    }

    [Test]
    [Arguments(DefaultTestEmail)]
    [Arguments(null)]
    public async Task GivenOrderCompletedEvent_WhenConsuming_ThenShouldPublishCompleteOrderCommandOnlyWithValidEmail(
        string? email
    )
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const decimal totalMoney = 100.0m;

        await InitializeSagaToPlacedState(orderId, basketId, email);

        var @event = new OrderStatusChangedToCompleteIntegrationEvent(
            orderId,
            basketId,
            email,
            totalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await AssertEventConsumed<OrderStatusChangedToCompleteIntegrationEvent>();

        var instance = AssertSagaInState(orderId, _sagaHarness.StateMachine.Completed);

        instance.OrderId.ShouldBe(orderId);
        instance.BasketId.ShouldBe(basketId);
        instance.TotalMoney.ShouldBe(totalMoney);
        instance.Email.ShouldBe(email);
        instance.Version.ShouldBeGreaterThanOrEqualTo(0);
        instance.CurrentState.ShouldBe(_sagaHarness.StateMachine.Completed.Name);
        instance.OrderPlacedDate.ShouldNotBe(DateTime.MinValue);

        if (email is not null)
        {
            (await _harness.Published.Any<CompleteOrderCommand>()).ShouldBeTrue();
            var publishedMessage = _harness.Published.Select<CompleteOrderCommand>().First();
            var message = publishedMessage.Context.Message;
            message.OrderId.ShouldBe(orderId);
            message.Email.ShouldBe(email);
            message.TotalMoney.ShouldBe(totalMoney);
        }
        else
        {
            (await _harness.Published.Any<CompleteOrderCommand>()).ShouldBeFalse();
        }
    }

    [Test]
    [Arguments(DefaultTestEmail)]
    [Arguments(null)]
    public async Task GivenOrderCancelledEvent_WhenConsuming_ThenShouldPublishCancelOrderCommandOnlyWithValidEmail(
        string? email
    )
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const decimal totalMoney = 100.0m;

        await InitializeSagaToPlacedState(orderId, basketId, email);

        var @event = new OrderStatusChangedToCancelIntegrationEvent(
            orderId,
            basketId,
            email,
            totalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await AssertEventConsumed<OrderStatusChangedToCancelIntegrationEvent>();

        var instance = AssertSagaInState(orderId, _sagaHarness.StateMachine.Cancelled);

        instance.OrderId.ShouldBe(orderId);
        instance.BasketId.ShouldBe(basketId);
        instance.TotalMoney.ShouldBe(totalMoney);
        instance.Email.ShouldBe(email);
        instance.Version.ShouldBeGreaterThanOrEqualTo(0);
        instance.CurrentState.ShouldBe(_sagaHarness.StateMachine.Cancelled.Name);
        instance.OrderPlacedDate.ShouldNotBe(DateTime.MinValue);

        if (email is not null)
        {
            (await _harness.Published.Any<CancelOrderCommand>()).ShouldBeTrue();
            var publishedMessage = _harness.Published.Select<CancelOrderCommand>().First();
            var message = publishedMessage.Context.Message;
            message.OrderId.ShouldBe(orderId);
            message.Email.ShouldBe(email);
            message.TotalMoney.ShouldBe(totalMoney);
        }
        else
        {
            (await _harness.Published.Any<CancelOrderCommand>()).ShouldBeFalse();
        }
    }
}
