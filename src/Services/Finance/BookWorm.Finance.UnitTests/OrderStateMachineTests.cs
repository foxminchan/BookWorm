using BookWorm.Contracts;
using BookWorm.Finance.Saga;
using BookWorm.Finance.UnitTests.Extensions;
using BookWorm.SharedKernel.Helpers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using static BookWorm.Finance.UnitTests.Helpers.OrderStateMachineTestHelper;

namespace BookWorm.Finance.UnitTests;

public sealed class OrderStateMachineTests
{
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
            {
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>();
                cfg.SetTestTimeouts(testInactivityTimeout: TimeSpan.FromSeconds(60));
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        _harness.TestInactivityTimeout = TimeSpan.FromSeconds(60);

        await _harness.Start();
        _sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
    }

    [After(Test)]
    public async Task TearDown()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Test]
    [Retry(2)]
    public async Task GivenUserCheckedOutIntegrationEvent_WhenConsuming_ThenShouldCreateOrderStateAndPublishPlaceOrderCommand()
    {
        // Arrange
        var testData = CreateTestOrderData();
        var @event = new UserCheckedOutIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            testData.FullName,
            testData.Email,
            testData.TotalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await _harness.AssertEventConsumed<UserCheckedOutIntegrationEvent>();
        (await _sagaHarness.Created.Any(x => x.CorrelationId == testData.OrderId)).ShouldBeTrue();

        var instance = _sagaHarness.AssertSagaInState(
            testData.OrderId,
            _sagaHarness.StateMachine.Placed
        );
        AssertOrderState(instance, testData);
        instance.CurrentState.ShouldBe(_sagaHarness.StateMachine.Placed.Name);

        var command = await _harness.AssertCommandPublished<PlaceOrderCommand>();
        command.BasketId.ShouldBe(testData.BasketId);
        command.OrderId.ShouldBe(testData.OrderId);
        command.FullName.ShouldBe(testData.FullName);
        command.Email.ShouldBe(testData.Email);
        command.TotalMoney.ShouldBe(testData.TotalMoney);
    }

    [Test]
    public async Task GivenBasketDeletedCompleteIntegrationEvent_WhenConsuming_ThenShouldPublishDeleteBasketCompleteCommand()
    {
        // Arrange
        var testData = CreateTestOrderData();
        await (_harness, _sagaHarness).InitializeSagaToPlacedState(testData);

        var @event = new BasketDeletedCompleteIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            testData.TotalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await _harness.AssertEventConsumed<BasketDeletedCompleteIntegrationEvent>();

        var instance = _sagaHarness.AssertSagaInState(
            testData.OrderId,
            _sagaHarness.StateMachine.Placed
        );
        AssertOrderState(instance, testData);
        instance.CurrentState.ShouldBe(_sagaHarness.StateMachine.Placed.Name);

        var command = await _harness.AssertCommandPublished<DeleteBasketCompleteCommand>();
        command.OrderId.ShouldBe(testData.OrderId);
        command.TotalMoney.ShouldBe(testData.TotalMoney);
    }

    [Test]
    public async Task GivenBasketDeletedFailedIntegrationEvent_WhenConsuming_ThenShouldPublishDeleteBasketFailedCommand()
    {
        // Arrange
        var testData = CreateTestOrderData();
        await (_harness, _sagaHarness).InitializeSagaToPlacedState(testData);

        var @event = new BasketDeletedFailedIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            testData.Email,
            testData.TotalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await _harness.AssertEventConsumed<BasketDeletedFailedIntegrationEvent>();

        var instance = _sagaHarness.AssertSagaInState(
            testData.OrderId,
            _sagaHarness.StateMachine.Failed
        );
        AssertOrderState(instance, testData);
        instance.CurrentState.ShouldBe(_sagaHarness.StateMachine.Failed.Name);

        var command = await _harness.AssertCommandPublished<DeleteBasketFailedCommand>();
        command.OrderId.ShouldBe(testData.OrderId);
        command.BasketId.ShouldBe(testData.BasketId);
        command.Email.ShouldBe(testData.Email);
        command.TotalMoney.ShouldBe(testData.TotalMoney);
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
        var testData = CreateTestOrderData(email: email);
        await (_harness, _sagaHarness).InitializeSagaToPlacedState(testData);

        var @event = new OrderStatusChangedToCompleteIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            testData.FullName,
            email,
            testData.TotalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await _harness.AssertEventConsumed<OrderStatusChangedToCompleteIntegrationEvent>();
        _sagaHarness.AssertSagaFinalized(testData.OrderId);

        if (email is not null)
        {
            var command = await _harness.AssertCommandPublished<CompleteOrderCommand>();
            command.OrderId.ShouldBe(testData.OrderId);
            command.FullName.ShouldBe(testData.FullName);
            command.Email.ShouldBe(email);
            command.TotalMoney.ShouldBe(testData.TotalMoney);
        }
        else
        {
            await _harness.AssertCommandNotPublished<CompleteOrderCommand>();
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
        var testData = CreateTestOrderData(email: email);
        await (_harness, _sagaHarness).InitializeSagaToPlacedState(testData);

        var @event = new OrderStatusChangedToCancelIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            testData.FullName,
            email,
            testData.TotalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await _harness.AssertEventConsumed<OrderStatusChangedToCancelIntegrationEvent>();
        await Task.Delay(100); // Small delay to ensure all processing is complete
        _sagaHarness.AssertSagaFinalized(testData.OrderId);

        if (email is not null)
        {
            var command = await _harness.AssertCommandPublished<CancelOrderCommand>();
            command.OrderId.ShouldBe(testData.OrderId);
            command.FullName.ShouldBe(testData.FullName);
            command.Email.ShouldBe(email);
            command.TotalMoney.ShouldBe(testData.TotalMoney);
        }
        else
        {
            await _harness.AssertCommandNotPublished<CancelOrderCommand>();
        }
    }

    [Test]
    public async Task GivenDuplicateOrderEvent_WhenConsuming_ThenShouldHandleGracefully()
    {
        // Arrange
        var testData = CreateTestOrderData();
        var checkoutEvent = new UserCheckedOutIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            testData.FullName,
            testData.Email,
            testData.TotalMoney
        );

        // Act - Send the first event
        await _harness.Bus.Publish(checkoutEvent);
        await _sagaHarness.Created.Any(x => x.CorrelationId == testData.OrderId);

        // Send the duplicate event
        await _harness.Bus.Publish(checkoutEvent);

        // Assert
        await _harness.AssertEventConsumed<UserCheckedOutIntegrationEvent>();
        (await _sagaHarness.Created.Any(x => x.CorrelationId == testData.OrderId)).ShouldBeTrue();

        var instance = _sagaHarness.AssertSagaInState(
            testData.OrderId,
            _sagaHarness.StateMachine.Placed
        );
        AssertOrderState(instance, testData);
        instance.CurrentState.ShouldBe(_sagaHarness.StateMachine.Placed.Name);

        // Should only publish PlaceOrderCommand once, not twice (key idempotency test)
        (await _harness.Published.Any<PlaceOrderCommand>()).ShouldBeTrue();
        var publishedCommands = _harness.Published.Select<PlaceOrderCommand>().ToList();
        publishedCommands.Count.ShouldBe(1);

        var command = publishedCommands[0].Context.Message;
        command.OrderId.ShouldBe(testData.OrderId);
        command.BasketId.ShouldBe(testData.BasketId);
        command.Email.ShouldBe(testData.Email);
        command.TotalMoney.ShouldBe(testData.TotalMoney);
    }

    [Test]
    public async Task GivenPlaceOrderCommand_WhenPublished_ThenShouldContainCorrectBusinessData()
    {
        // Arrange
        var testData = CreateTestOrderData(
            fullName: "Jane Smith",
            email: "jane.smith@example.com",
            totalMoney: 299.99m
        );

        var checkoutEvent = new UserCheckedOutIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            testData.FullName,
            testData.Email,
            testData.TotalMoney
        );

        // Act
        await _harness.Bus.Publish(checkoutEvent);

        // Assert
        await _harness.AssertEventConsumed<UserCheckedOutIntegrationEvent>();
        (await _sagaHarness.Created.Any(x => x.CorrelationId == testData.OrderId)).ShouldBeTrue();

        var instance = _sagaHarness.AssertSagaInState(
            testData.OrderId,
            _sagaHarness.StateMachine.Placed
        );

        var command = await _harness.AssertCommandPublished<PlaceOrderCommand>();
        command.OrderId.ShouldBe(testData.OrderId);
        command.BasketId.ShouldBe(testData.BasketId);
        command.FullName.ShouldBe(testData.FullName);
        command.Email.ShouldBe(testData.Email);
        command.TotalMoney.ShouldBe(testData.TotalMoney);

        // Verify command has proper message envelope
        var publishedMessage = _harness.Published.Select<PlaceOrderCommand>().First();
        publishedMessage.Context.Message.ShouldNotBeNull();
        publishedMessage.Context.MessageId.ShouldNotBeNull();

        // Validate saga state reflects the command execution
        AssertOrderState(instance, testData);
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
        await _harness.AssertEventConsumed<UserCheckedOutIntegrationEvent>();

        // Each order should have exactly one saga instance
        foreach (var orderId in orderIds)
        {
            (
                await _sagaHarness.Created.Any(x => x.CorrelationId == orderId, cancellationToken)
            ).ShouldBeTrue();
            var instance = _sagaHarness.AssertSagaInState(
                orderId,
                _sagaHarness.StateMachine.Placed
            );

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
        var testData = CreateTestOrderData();
        await (_harness, _sagaHarness).InitializeSagaToPlacedState(testData);

        var timeoutEvent = new PlaceOrderTimeoutIntegrationEvent(testData.OrderId);

        // Act - Send first timeout (should trigger retry)
        await _harness.Bus.Publish(timeoutEvent);

        // Assert
        await _harness.AssertEventConsumed<PlaceOrderTimeoutIntegrationEvent>();

        var instance = _sagaHarness.AssertSagaInState(
            testData.OrderId,
            _sagaHarness.StateMachine.Placed
        );
        instance.TimeoutRetryCount.ShouldBe(1);
        instance.RowVersion.ShouldBeGreaterThanOrEqualTo(0u);

        // Should have published another PlaceOrderCommand (retry)
        var publishedCommands = _harness.Published.Select<PlaceOrderCommand>().ToList();
        publishedCommands.Count.ShouldBe(2); // Initial + 1 retry

        var retryCommand = publishedCommands[^1].Context.Message;
        retryCommand.OrderId.ShouldBe(testData.OrderId);
        retryCommand.BasketId.ShouldBe(testData.BasketId);
        retryCommand.TotalMoney.ShouldBe(testData.TotalMoney);
    }

    [Test]
    [Retry(3)]
    public async Task GivenOrderTimeout_WhenRetryCountReachesMax_ThenShouldTransitionToFailedAndCancel()
    {
        // Arrange
        var testData = CreateTestOrderData();
        await (_harness, _sagaHarness).InitializeSagaToPlacedState(testData);

        var timeoutEvent = new PlaceOrderTimeoutIntegrationEvent(testData.OrderId);

        // Act - Send 3 timeouts to exhaust retries
        for (var i = 0; i < 3; i++)
        {
            await _harness.Bus.Publish(timeoutEvent);
            await Task.Delay(50); // Small delay between timeout events to ensure proper processing
        }

        // Assert
        await _harness.AssertEventConsumed<PlaceOrderTimeoutIntegrationEvent>();

        var command = await _harness.AssertCommandPublished<CancelOrderCommand>();
        command.OrderId.ShouldBe(testData.OrderId);
        command.FullName.ShouldBe(testData.FullName);
        command.Email.ShouldBe(testData.Email);
        command.TotalMoney.ShouldBe(testData.TotalMoney);

        // Verify the saga transitioned to Failed state or was finalized
        var instance = _sagaHarness.Created.ContainsInState(
            testData.OrderId,
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
            _sagaHarness.AssertSagaFinalized(testData.OrderId);
        }
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
        var testData = CreateTestOrderData(email: email, fullName: fullName);
        await (_harness, _sagaHarness).InitializeSagaToPlacedState(testData);

        var @event = new OrderStatusChangedToCompleteIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            fullName,
            email,
            testData.TotalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await Task.Delay(1000); // Give extra time for CI environments
        using var eventCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        (
            await _harness.Consumed.Any<OrderStatusChangedToCompleteIntegrationEvent>(
                eventCts.Token
            )
        ).ShouldBeTrue();

        // Should not publish CompleteOrderCommand when FullName is null/empty
        await _harness.AssertCommandNotPublished<CompleteOrderCommand>();
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
        var testData = CreateTestOrderData(email: email, fullName: fullName);
        await (_harness, _sagaHarness).InitializeSagaToPlacedState(testData);

        var @event = new OrderStatusChangedToCancelIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            fullName,
            email,
            testData.TotalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await Task.Delay(1000); // Give extra time for CI environments
        using var eventCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        (
            await _harness.Consumed.Any<OrderStatusChangedToCancelIntegrationEvent>(eventCts.Token)
        ).ShouldBeTrue();

        // Should not publish CancelOrderCommand when FullName is null/empty
        await _harness.AssertCommandNotPublished<CancelOrderCommand>();
    }

    [Test]
    [Arguments(null, DefaultTestFullName)]
    [Arguments(DefaultTestEmail, null)]
    [Arguments(null, null)]
    public async Task GivenOrderStatusChangedToCancelEvent_WhenEmailOrFullNameIsNull_ThenShouldNotPublishCancelOrderCommand(
        string? email,
        string? fullName
    )
    {
        // Arrange
        var testData = CreateTestOrderData(email: email, fullName: fullName);
        await (_harness, _sagaHarness).InitializeSagaToPlacedState(testData);

        var @event = new OrderStatusChangedToCancelIntegrationEvent(
            testData.OrderId,
            testData.BasketId,
            email,
            fullName,
            testData.TotalMoney
        );

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        await Task.Delay(1000); // Give extra time for CI environments
        using var eventCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        (
            await _harness.Consumed.Any<OrderStatusChangedToCancelIntegrationEvent>(eventCts.Token)
        ).ShouldBeTrue();

        // Should not publish CancelOrderCommand when FullName is null/empty
        await _harness.AssertCommandNotPublished<CancelOrderCommand>();
    }
}
