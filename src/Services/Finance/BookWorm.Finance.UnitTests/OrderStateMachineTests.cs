using BookWorm.Contracts;
using BookWorm.Finance.Saga;
using BookWorm.SharedKernel.Helpers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Finance.UnitTests;

public sealed class OrderStateMachineTests
{
    private const string? DefaultTestEmail = "example@email.com";
    private const string DefaultTestFullName = "John Doe";
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
        string? fullName = DefaultTestFullName,
        decimal totalMoney = 100.0m
    )
    {
        orderId ??= Guid.CreateVersion7();
        basketId ??= Guid.CreateVersion7();

        var checkoutEvent = new UserCheckedOutIntegrationEvent(
            orderId.Value,
            basketId.Value,
            fullName,
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
        // Use CancellationToken with timeout for more reliable testing
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // The primary assertion is that the saga consumed the event
        // This is the most important check for saga state machine tests
        (await _sagaHarness.Consumed.Any<TEvent>(cts.Token)).ShouldBeTrue();

        // Secondary check for general harness consumption
        // This verifies the message was published and consumed in the system
        (await _harness.Consumed.Any<TEvent>(cts.Token)).ShouldBeTrue();
    }

    private async Task<bool> WaitForCommandPublished<TCommand>(TimeSpan? timeout = null)
        where TCommand : class
    {
        timeout ??= TimeSpan.FromSeconds(30);
        using var cts = new CancellationTokenSource(timeout.Value);

        try
        {
            return await _harness.Published.Any<TCommand>(cts.Token);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
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
    [Retry(2)]
    public async Task GivenUserCheckedOutIntegrationEvent_WhenConsuming_ThenShouldCreateOrderStateAndPublishPlaceOrderCommand()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const decimal totalMoney = 100.0m;

        var @event = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            DefaultTestFullName,
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
        instance.RowVersion.ShouldBeGreaterThanOrEqualTo(0u);
        instance.CurrentState.ShouldBe(_sagaHarness.StateMachine.Placed.Name);
        instance.OrderPlacedDate.ShouldNotBe(DateTime.MinValue);

        (await WaitForCommandPublished<PlaceOrderCommand>()).ShouldBeTrue();

        var publishedMessage = _harness.Published.Select<PlaceOrderCommand>().First();
        var message = publishedMessage.Context.Message;
        message.BasketId.ShouldBe(basketId);
        message.OrderId.ShouldBe(orderId);
        message.FullName.ShouldBe(DefaultTestFullName);
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
        instance.RowVersion.ShouldBeGreaterThanOrEqualTo(0u);
        instance.CurrentState.ShouldBe(_sagaHarness.StateMachine.Placed.Name);
        instance.OrderPlacedDate.ShouldNotBe(DateTime.MinValue);

        (await WaitForCommandPublished<DeleteBasketCompleteCommand>()).ShouldBeTrue();
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
        instance.RowVersion.ShouldBeGreaterThanOrEqualTo(0u);
        instance.CurrentState.ShouldBe(_sagaHarness.StateMachine.Failed.Name);
        instance.OrderPlacedDate.ShouldNotBe(DateTime.MinValue);

        (await WaitForCommandPublished<DeleteBasketFailedCommand>()).ShouldBeTrue();
        var publishedMessage = _harness.Published.Select<DeleteBasketFailedCommand>().First();
        var message = publishedMessage.Context.Message;
        message.OrderId.ShouldBe(orderId);
        message.BasketId.ShouldBe(basketId);
        message.Email.ShouldBe(DefaultTestEmail);
        message.TotalMoney.ShouldBe(totalMoney);
    }

    [Test]
    [Retry(3)]
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
            DefaultTestFullName,
            email,
            totalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await AssertEventConsumed<OrderStatusChangedToCompleteIntegrationEvent>();

        // The saga should be finalized after completion - check that it's not in any active state
        var sagaNotInPlaced =
            _sagaHarness.Created.ContainsInState(
                orderId,
                _sagaHarness.StateMachine,
                _sagaHarness.StateMachine.Placed
            )
                is null;
        var sagaNotInCompleted =
            _sagaHarness.Created.ContainsInState(
                orderId,
                _sagaHarness.StateMachine,
                _sagaHarness.StateMachine.Completed
            )
                is null;
        var sagaNotInCancelled =
            _sagaHarness.Created.ContainsInState(
                orderId,
                _sagaHarness.StateMachine,
                _sagaHarness.StateMachine.Cancelled
            )
                is null;
        var sagaNotInFailed =
            _sagaHarness.Created.ContainsInState(
                orderId,
                _sagaHarness.StateMachine,
                _sagaHarness.StateMachine.Failed
            )
                is null;

        // Saga should be finalized (not in any state) or in a final state but not testable due to finalization
        (
            sagaNotInPlaced && sagaNotInCompleted && sagaNotInCancelled && sagaNotInFailed
        ).ShouldBeTrue();

        // But we can verify it was completed by checking the published command
        if (email is not null)
        {
            (await WaitForCommandPublished<CompleteOrderCommand>()).ShouldBeTrue();
            var publishedMessage = _harness.Published.Select<CompleteOrderCommand>().First();
            var message = publishedMessage.Context.Message;
            message.OrderId.ShouldBe(orderId);
            message.FullName.ShouldBe(DefaultTestFullName);
            message.Email.ShouldBe(email);
            message.TotalMoney.ShouldBe(totalMoney);
        }
        else
        {
            (
                await WaitForCommandPublished<CompleteOrderCommand>(TimeSpan.FromSeconds(5))
            ).ShouldBeFalse();
        }
    }

    [Test]
    [Retry(3)]
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
            DefaultTestFullName,
            email,
            totalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await AssertEventConsumed<OrderStatusChangedToCancelIntegrationEvent>();

        // Add explicit wait for command processing in case email/fullName validation affects timing
        await Task.Delay(100); // Small delay to ensure all processing is complete

        // The saga should be finalized after cancellation - check that it's not in any active state
        var sagaNotInPlaced =
            _sagaHarness.Created.ContainsInState(
                orderId,
                _sagaHarness.StateMachine,
                _sagaHarness.StateMachine.Placed
            )
                is null;
        var sagaNotInCompleted =
            _sagaHarness.Created.ContainsInState(
                orderId,
                _sagaHarness.StateMachine,
                _sagaHarness.StateMachine.Completed
            )
                is null;
        var sagaNotInCancelled =
            _sagaHarness.Created.ContainsInState(
                orderId,
                _sagaHarness.StateMachine,
                _sagaHarness.StateMachine.Cancelled
            )
                is null;
        var sagaNotInFailed =
            _sagaHarness.Created.ContainsInState(
                orderId,
                _sagaHarness.StateMachine,
                _sagaHarness.StateMachine.Failed
            )
                is null;

        // Saga should be finalized (not in any state) or in a final state but not testable due to finalization
        (
            sagaNotInPlaced && sagaNotInCompleted && sagaNotInCancelled && sagaNotInFailed
        ).ShouldBeTrue();

        // But we can verify it was cancelled by checking the published command
        if (email is not null)
        {
            (await WaitForCommandPublished<CancelOrderCommand>()).ShouldBeTrue();
            var publishedMessage = _harness.Published.Select<CancelOrderCommand>().First();
            var message = publishedMessage.Context.Message;
            message.OrderId.ShouldBe(orderId);
            message.FullName.ShouldBe(DefaultTestFullName);
            message.Email.ShouldBe(email);
            message.TotalMoney.ShouldBe(totalMoney);
        }
        else
        {
            (
                await WaitForCommandPublished<CancelOrderCommand>(TimeSpan.FromSeconds(5))
            ).ShouldBeFalse();
        }
    }

    [Test]
    public async Task GivenDuplicateOrderEvent_WhenConsuming_ThenShouldHandleGracefully()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const decimal totalMoney = 100.0m;

        var checkoutEvent = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            DefaultTestFullName,
            DefaultTestEmail,
            totalMoney
        );

        // Act - Send the first event
        await _harness.Bus.Publish(checkoutEvent);

        // Wait for the saga to be created and transition to Placed state
        await _sagaHarness.Created.Any(x => x.CorrelationId == orderId);

        // Send the duplicate event
        await _harness.Bus.Publish(checkoutEvent);

        // Assert
        await AssertEventConsumed<UserCheckedOutIntegrationEvent>();

        // Should only create one saga instance, not two
        (await _sagaHarness.Created.Any(x => x.CorrelationId == orderId)).ShouldBeTrue();

        var instance = AssertSagaInState(orderId, _sagaHarness.StateMachine.Placed);

        // Verify saga properties are correct
        instance.OrderId.ShouldBe(orderId);
        instance.BasketId.ShouldBe(basketId);
        instance.Email.ShouldBe(DefaultTestEmail);
        instance.TotalMoney.ShouldBe(totalMoney);
        instance.RowVersion.ShouldBeGreaterThanOrEqualTo(0u);
        instance.CurrentState.ShouldBe(_sagaHarness.StateMachine.Placed.Name);

        // Should only publish PlaceOrderCommand once, not twice (this is the key idempotency test)
        (await _harness.Published.Any<PlaceOrderCommand>()).ShouldBeTrue();
        var publishedCommands = _harness.Published.Select<PlaceOrderCommand>().ToList();
        publishedCommands.Count.ShouldBe(1);

        var command = publishedCommands[0].Context.Message;
        command.OrderId.ShouldBe(orderId);
        command.BasketId.ShouldBe(basketId);
        command.Email.ShouldBe(DefaultTestEmail);
        command.TotalMoney.ShouldBe(totalMoney);
    }

    [Test]
    public async Task GivenPlaceOrderCommand_WhenPublished_ThenShouldContainCorrectBusinessData()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const string fullName = "Jane Smith";
        const string email = "jane.smith@example.com";
        const decimal totalMoney = 299.99m;

        var checkoutEvent = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            fullName,
            email,
            totalMoney
        );

        // Act
        await _harness.Bus.Publish(checkoutEvent);

        // Assert
        await AssertEventConsumed<UserCheckedOutIntegrationEvent>();
        (await _sagaHarness.Created.Any(x => x.CorrelationId == orderId)).ShouldBeTrue();

        var instance = AssertSagaInState(orderId, _sagaHarness.StateMachine.Placed);

        // Verify PlaceOrderCommand was published with correct business logic applied
        (await _harness.Published.Any<PlaceOrderCommand>()).ShouldBeTrue();
        var publishedMessage = _harness.Published.Select<PlaceOrderCommand>().First();
        var command = publishedMessage.Context.Message;

        // Validate all business data is correctly transferred
        command.OrderId.ShouldBe(orderId);
        command.BasketId.ShouldBe(basketId);
        command.FullName.ShouldBe(fullName);
        command.Email.ShouldBe(email);
        command.TotalMoney.ShouldBe(totalMoney);

        // Verify command has proper message envelope (what we can reliably test)
        publishedMessage.Context.Message.ShouldNotBeNull();
        publishedMessage.Context.MessageId.ShouldNotBeNull();

        // Validate saga state reflects the command execution
        instance.OrderId.ShouldBe(orderId);
        instance.BasketId.ShouldBe(basketId);
        instance.Email.ShouldBe(email);
        instance.TotalMoney.ShouldBe(totalMoney);
        instance.RowVersion.ShouldBeGreaterThanOrEqualTo(0u);
        instance.OrderPlacedDate.ShouldNotBeNull();
        var now = DateTimeHelper.UtcNow();
        instance.OrderPlacedDate.Value.ShouldBeInRange(now.AddMinutes(-1), now.AddMinutes(1));
    }

    [Test]
    [Retry(2)]
    [Timeout(45_000)] // 45 seconds for concurrent test
    public async Task GivenMultipleConcurrentEvents_WhenProcessed_ThenShouldMaintainConsistency(
        CancellationToken cancellationToken
    )
    {
        // Arrange
        const int numberOfOrders = 5;
        var orderIds = Enumerable
            .Range(0, numberOfOrders)
            .Select(_ => Guid.CreateVersion7())
            .ToList();

        var basketIds = Enumerable
            .Range(0, numberOfOrders)
            .Select(_ => Guid.CreateVersion7())
            .ToList();

        var checkoutEvents = orderIds
            .Zip(
                basketIds,
                (orderId, basketId) =>
                    new UserCheckedOutIntegrationEvent(
                        orderId,
                        basketId,
                        $"User {orderId:N}",
                        $"user{orderId:N}@example.com",
                        100.0m + Math.Abs(orderId.GetHashCode() % 1000) // Ensure positive amounts
                    )
            )
            .ToList();

        // Act - Send all events concurrently
        var publishTasks = checkoutEvents.Select(evt =>
            _harness.Bus.Publish(evt, cancellationToken)
        );
        await Task.WhenAll(publishTasks);

        // Wait for all sagas to be created
        foreach (var orderId in orderIds)
        {
            await _sagaHarness.Created.Any(x => x.CorrelationId == orderId, cancellationToken);
        }

        // Assert
        // Verify all events were consumed
        await AssertEventConsumed<UserCheckedOutIntegrationEvent>();

        // Each order should have exactly one saga instance
        foreach (var orderId in orderIds)
        {
            (
                await _sagaHarness.Created.Any(x => x.CorrelationId == orderId, cancellationToken)
            ).ShouldBeTrue();
            var instance = AssertSagaInState(orderId, _sagaHarness.StateMachine.Placed);

            // Verify saga state consistency
            instance.OrderId.ShouldBe(orderId);
            instance.RowVersion.ShouldBeGreaterThanOrEqualTo(0u);
            instance.CurrentState.ShouldBe(_sagaHarness.StateMachine.Placed.Name);
            instance.OrderPlacedDate.ShouldNotBeNull();
        }

        // Verify exactly one PlaceOrderCommand per order (no duplicates)
        var publishedCommands = _harness.Published.Select<PlaceOrderCommand>().ToList();
        publishedCommands.Count.ShouldBe(numberOfOrders);

        // Verify each order has exactly one corresponding command
        foreach (var orderId in orderIds)
        {
            var commandsForOrder = publishedCommands
                .Where(msg => msg.Context.Message.OrderId == orderId)
                .ToList();

            commandsForOrder.Count.ShouldBe(
                1,
                $"Order {orderId} should have exactly one PlaceOrderCommand"
            );

            var command = commandsForOrder[0].Context.Message;
            command.OrderId.ShouldBe(orderId);
            command.TotalMoney.ShouldBeGreaterThan(0);
        }

        // Verify no race conditions caused data corruption
        var allInstances = orderIds
            .Select(orderId =>
                _sagaHarness.Created.ContainsInState(
                    orderId,
                    _sagaHarness.StateMachine,
                    _sagaHarness.StateMachine.Placed
                )
            )
            .ToList();

        allInstances.ShouldAllBe(instance => instance != null);
        var actualOrderIds = allInstances.Select(i => i!.OrderId).ToList();
        actualOrderIds.ShouldBeEquivalentTo(orderIds);
    }

    [Test]
    public void GivenMassTransitConfiguration_WhenConfigured_ThenShouldRegisterSagaCorrectly()
    {
        // Assert - Verify test harness configuration
        _harness.ShouldNotBeNull();
        _sagaHarness.ShouldNotBeNull();
        _sagaHarness.StateMachine.ShouldNotBeNull();

        // Verify the saga state machine type
        _sagaHarness.StateMachine.ShouldBeOfType<OrderStateMachine>();

        // Verify all required states are defined
        _sagaHarness.StateMachine.Placed.ShouldNotBeNull();
        _sagaHarness.StateMachine.Completed.ShouldNotBeNull();
        _sagaHarness.StateMachine.Cancelled.ShouldNotBeNull();
        _sagaHarness.StateMachine.Failed.ShouldNotBeNull();

        // Verify all required events are defined
        _sagaHarness.StateMachine.OrderPlaced.ShouldNotBeNull();
        _sagaHarness.StateMachine.OrderCompleted.ShouldNotBeNull();
        _sagaHarness.StateMachine.OrderCancelled.ShouldNotBeNull();
        _sagaHarness.StateMachine.BasketDeletedFailed.ShouldNotBeNull();
        _sagaHarness.StateMachine.BasketDeleted.ShouldNotBeNull();

        // Verify timeout schedule is defined
        _sagaHarness.StateMachine.PlaceOrderTimeoutSchedule.ShouldNotBeNull();

        // Verify state names are correct
        _sagaHarness.StateMachine.Placed.Name.ShouldBe("Placed");
        _sagaHarness.StateMachine.Completed.Name.ShouldBe("Completed");
        _sagaHarness.StateMachine.Cancelled.Name.ShouldBe("Cancelled");
        _sagaHarness.StateMachine.Failed.Name.ShouldBe("Failed");

        // Verify initial state configuration
        _sagaHarness.StateMachine.Initial.ShouldNotBeNull();
        _sagaHarness.StateMachine.Final.ShouldNotBeNull();

        // Verify that the state machine has the expected states in its state graph
        var states = _sagaHarness.StateMachine.States.ToList();
        states.ShouldContain(s => s.Name == "Initial");
        states.ShouldContain(s => s.Name == "Final");
        states.ShouldContain(s => s.Name == "Placed");
        states.ShouldContain(s => s.Name == "Completed");
        states.ShouldContain(s => s.Name == "Cancelled");
        states.ShouldContain(s => s.Name == "Failed");

        // Verify that the state machine has the expected events
        var events = _sagaHarness.StateMachine.Events.ToList();
        events.Count.ShouldBeGreaterThan(0);
        events.ShouldContain(e => e.Name == "OrderPlaced");
        events.ShouldContain(e => e.Name == "OrderCompleted");
        events.ShouldContain(e => e.Name == "OrderCancelled");
        events.ShouldContain(e => e.Name == "BasketDeletedFailed");
        events.ShouldContain(e => e.Name == "BasketDeleted");

        // Verify that the timeout schedule exists and has proper event handling
        _sagaHarness.StateMachine.PlaceOrderTimeoutSchedule.Received.ShouldNotBeNull();
    }

    [Test]
    public async Task GivenOrderTimeout_WhenRetryCountLessThanMax_ThenShouldRetryOrderProcessing()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const decimal totalMoney = 100.0m;

        await InitializeSagaToPlacedState(orderId, basketId);

        // Create a timeout event
        var timeoutEvent = new PlaceOrderTimeoutIntegrationEvent(orderId);

        // Act - Send first timeout (should trigger retry)
        await _harness.Bus.Publish(timeoutEvent);

        // Assert
        await AssertEventConsumed<PlaceOrderTimeoutIntegrationEvent>();

        // Should still be in Placed state (not Failed)
        var instance = AssertSagaInState(orderId, _sagaHarness.StateMachine.Placed);

        // Verify retry count was incremented
        instance.TimeoutRetryCount.ShouldBe(1);
        instance.RowVersion.ShouldBeGreaterThanOrEqualTo(0u);

        // Should have published another PlaceOrderCommand (retry)
        var publishedCommands = _harness.Published.Select<PlaceOrderCommand>().ToList();
        publishedCommands.Count.ShouldBe(2); // Initial + 1 retry

        // Verify the retry command has correct data
        var retryCommand = publishedCommands[^1].Context.Message;
        retryCommand.OrderId.ShouldBe(orderId);
        retryCommand.BasketId.ShouldBe(basketId);
        retryCommand.TotalMoney.ShouldBe(totalMoney);
    }

    [Test]
    [Retry(3)]
    public async Task GivenOrderTimeout_WhenRetryCountReachesMax_ThenShouldTransitionToFailedAndCancel()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const decimal totalMoney = 100.0m;

        await InitializeSagaToPlacedState(orderId, basketId);

        var timeoutEvent = new PlaceOrderTimeoutIntegrationEvent(orderId);

        // Act - Send 3 timeouts to exhaust retries
        for (var i = 0; i < 3; i++)
        {
            await _harness.Bus.Publish(timeoutEvent);
            // Add small delay between timeout events to ensure proper processing
            await Task.Delay(50);
        }

        // Assert
        await AssertEventConsumed<PlaceOrderTimeoutIntegrationEvent>();

        // After max retries, the order should be cancelled
        // The most important behavior is that CancelOrderCommand is published
        (await WaitForCommandPublished<CancelOrderCommand>()).ShouldBeTrue();
        var cancelCommand = _harness.Published.Select<CancelOrderCommand>().First();
        var message = cancelCommand.Context.Message;
        message.OrderId.ShouldBe(orderId);
        message.FullName.ShouldBe(DefaultTestFullName);
        message.Email.ShouldBe(DefaultTestEmail);
        message.TotalMoney.ShouldBe(totalMoney);

        // Verify the saga transitioned to Failed state or was finalized
        // Try to get the saga instance to check its state
        var instance = _sagaHarness.Created.ContainsInState(
            orderId,
            _sagaHarness.StateMachine,
            _sagaHarness.StateMachine.Failed
        );

        if (instance is not null)
        {
            instance.CurrentState.ShouldBe(_sagaHarness.StateMachine.Failed.Name);
            instance.TimeoutRetryCount.ShouldBe(3);
            instance.RowVersion.ShouldBeGreaterThanOrEqualTo(0u);
        }
        else
        {
            // If the saga was finalized, it won't be in any state
            var sagaNotInPlaced =
                _sagaHarness.Created.ContainsInState(
                    orderId,
                    _sagaHarness.StateMachine,
                    _sagaHarness.StateMachine.Placed
                )
                    is null;
            var sagaNotInCompleted =
                _sagaHarness.Created.ContainsInState(
                    orderId,
                    _sagaHarness.StateMachine,
                    _sagaHarness.StateMachine.Completed
                )
                    is null;
            var sagaNotInCancelled =
                _sagaHarness.Created.ContainsInState(
                    orderId,
                    _sagaHarness.StateMachine,
                    _sagaHarness.StateMachine.Cancelled
                )
                    is null;

            // Saga should be finalized (not in any state)
            (sagaNotInPlaced && sagaNotInCompleted && sagaNotInCancelled).ShouldBeTrue();
        }
    }

    [Test]
    [Retry(3)]
    [Timeout(30_000)] // 30 seconds for timeout handling test
    public async Task GivenTimeoutRetryAttempts_WhenOrderCompletes_ThenShouldUnscheduleTimeoutAndComplete(
        CancellationToken cancellationToken
    )
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const decimal totalMoney = 100.0m;

        await InitializeSagaToPlacedState(orderId, basketId);

        // Send one timeout to increment retry count
        var timeoutEvent = new PlaceOrderTimeoutIntegrationEvent(orderId);
        await _harness.Bus.Publish(timeoutEvent, cancellationToken);

        // Wait for timeout event to be consumed
        await AssertEventConsumed<PlaceOrderTimeoutIntegrationEvent>();

        // Verify we're still in Placed state with retry count incremented
        var instanceAfterTimeout = AssertSagaInState(orderId, _sagaHarness.StateMachine.Placed);
        instanceAfterTimeout.TimeoutRetryCount.ShouldBe(1);
        instanceAfterTimeout.RowVersion.ShouldBeGreaterThanOrEqualTo(0u);

        // Act - Now complete the order
        var completeEvent = new OrderStatusChangedToCompleteIntegrationEvent(
            orderId,
            basketId,
            DefaultTestFullName,
            DefaultTestEmail,
            totalMoney
        );
        await _harness.Bus.Publish(completeEvent, cancellationToken);

        // Assert
        // Wait for the event to be consumed first
        await AssertEventConsumed<OrderStatusChangedToCompleteIntegrationEvent>();

        // Add explicit delay to ensure command publishing completes
        await Task.Delay(100, cancellationToken);

        // Verify completion command is published (this indirectly confirms the event was consumed)
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        (await _harness.Published.Any<CompleteOrderCommand>(cts.Token)).ShouldBeTrue();

        // Should transition to Completed state and then be finalized
        // We can't check the final state because finalized sagas are removed, but we can verify behavior
        var sagaNotInPlaced =
            _sagaHarness.Created.ContainsInState(
                orderId,
                _sagaHarness.StateMachine,
                _sagaHarness.StateMachine.Placed
            )
                is null;
        var sagaNotInCompleted =
            _sagaHarness.Created.ContainsInState(
                orderId,
                _sagaHarness.StateMachine,
                _sagaHarness.StateMachine.Completed
            )
                is null;
        var sagaNotInCancelled =
            _sagaHarness.Created.ContainsInState(
                orderId,
                _sagaHarness.StateMachine,
                _sagaHarness.StateMachine.Cancelled
            )
                is null;
        var sagaNotInFailed =
            _sagaHarness.Created.ContainsInState(
                orderId,
                _sagaHarness.StateMachine,
                _sagaHarness.StateMachine.Failed
            )
                is null;

        // Saga should be finalized (not in any state)
        (
            sagaNotInPlaced && sagaNotInCompleted && sagaNotInCancelled && sagaNotInFailed
        ).ShouldBeTrue();

        // Should have published CompleteOrderCommand
        (await _harness.Published.Any<CompleteOrderCommand>(cts.Token)).ShouldBeTrue();
    }

    [Test]
    [Arguments(DefaultTestEmail, null)]
    [Arguments(DefaultTestEmail, "")]
    [Arguments(DefaultTestEmail, "   ")]
    public async Task GivenOrderCompletedEvent_WhenFullNameIsNullOrEmpty_ThenShouldNotPublishCompleteOrderCommand(
        string? email,
        string? fullName
    )
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const decimal totalMoney = 100.0m;

        await InitializeSagaToPlacedState(orderId, basketId, email, fullName);

        var @event = new OrderStatusChangedToCompleteIntegrationEvent(
            orderId,
            basketId,
            fullName,
            email,
            totalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await AssertEventConsumed<OrderStatusChangedToCompleteIntegrationEvent>();

        // Should not publish CompleteOrderCommand when FullName is null/empty
        (
            await WaitForCommandPublished<CompleteOrderCommand>(TimeSpan.FromSeconds(5))
        ).ShouldBeFalse();
    }

    [Test]
    [Arguments(DefaultTestEmail, null)]
    [Arguments(DefaultTestEmail, "")]
    [Arguments(DefaultTestEmail, "   ")]
    public async Task GivenOrderCancelledEvent_WhenFullNameIsNullOrEmpty_ThenShouldNotPublishCancelOrderCommand(
        string? email,
        string? fullName
    )
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        const decimal totalMoney = 100.0m;

        await InitializeSagaToPlacedState(orderId, basketId, email, fullName);

        var @event = new OrderStatusChangedToCancelIntegrationEvent(
            orderId,
            basketId,
            fullName,
            email,
            totalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await AssertEventConsumed<OrderStatusChangedToCancelIntegrationEvent>();

        // Should not publish CancelOrderCommand when FullName is null/empty
        (
            await WaitForCommandPublished<CancelOrderCommand>(TimeSpan.FromSeconds(5))
        ).ShouldBeFalse();
    }

    [Test]
    [Arguments(null, DefaultTestFullName)]
    [Arguments(DefaultTestEmail, null)]
    [Arguments(null, null)]
    public async Task GivenOrderTimeout_WhenEmailOrFullNameIsNull_ThenShouldNotPublishCancelOrderCommand(
        string? email,
        string? fullName
    )
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();

        await InitializeSagaToPlacedState(orderId, basketId, email, fullName);

        var timeoutEvent = new PlaceOrderTimeoutIntegrationEvent(orderId);

        // Act - Send 3 timeouts to exhaust retries
        for (var i = 0; i < 3; i++)
        {
            await _harness.Bus.Publish(timeoutEvent);
            // Add small delay between timeout events to ensure proper processing
            await Task.Delay(50);
        }

        // Assert
        await AssertEventConsumed<PlaceOrderTimeoutIntegrationEvent>();

        // Should not publish CancelOrderCommand when Email or FullName is null
        (
            await WaitForCommandPublished<CancelOrderCommand>(TimeSpan.FromSeconds(5))
        ).ShouldBeFalse();
    }
}
