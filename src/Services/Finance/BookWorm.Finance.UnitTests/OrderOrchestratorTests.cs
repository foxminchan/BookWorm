using BookWorm.Contracts;
using BookWorm.Finance.Features.Orders.BasketDeletion;
using BookWorm.Finance.Features.Orders.Orchestration;
using BookWorm.Finance.Features.Orders.Orchestration.Models;
using BookWorm.Finance.Features.Orders.Orchestration.Saga;
using BookWorm.Finance.Features.Orders.OrderCancellation;
using BookWorm.Finance.Features.Orders.OrderCompletion;
using BookWorm.Finance.Features.Orders.PlaceOrder;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookWorm.Finance.UnitTests;

public sealed class OrderOrchestratorTests
{
    private static readonly DateTime _fixedUtcNow = new(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public async Task GivenHappyPath_WhenBasketDeletedAndOrderCompleted_ThenShouldPublishCommandsAndComplete()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var input = CreateInput(orderId, basketId);
        var harness = new OrchestratorTestHarness(input);

        var runTask = new OrderOrchestrator().RunAsync(harness.Context.Object, input);

        harness.CompleteExternalEvent(
            OrderOrchestrationExternalEvents.BasketDeletedComplete,
            new BasketDeletedCompleteIntegrationEvent(orderId, basketId, 75.25m)
        );
        await harness.WaitForActivityCountAsync(2);
        harness.CompleteExternalEvent(
            OrderOrchestrationExternalEvents.OrderCompleted,
            new OrderStatusChangedToCompleteIntegrationEvent(
                orderId,
                basketId,
                "Jane Doe",
                "jane@example.com",
                75.25m
            )
        );

        var result = await runTask;

        result.OrderId.ShouldBe(orderId);
        result.BasketId.ShouldBe(basketId);
        result.FullName.ShouldBe("Jane Doe");
        result.Email.ShouldBe("jane@example.com");
        result.TotalMoney.ShouldBe(75.25m);
        result.FinalStatus.ShouldBe(OrderOrchestrationStatus.Completed);
        result.TimeoutRetryCount.ShouldBe(0);
        result.OrderPlacedDate.ShouldBe(_fixedUtcNow);

        harness
            .CalledActivities.Select(a => a.Name)
            .ShouldBe([
                nameof(PublishPlaceOrderActivity),
                nameof(PublishDeleteBasketCompleteActivity),
                nameof(PublishCompleteOrderActivity),
            ]);
    }

    [Test]
    public async Task GivenBasketDeletionFailed_WhenEventReceived_ThenShouldPublishFailedCommandAndFinalize()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var input = CreateInput(orderId, basketId);
        var harness = new OrchestratorTestHarness(input);

        var runTask = new OrderOrchestrator().RunAsync(harness.Context.Object, input);

        harness.CompleteExternalEvent(
            OrderOrchestrationExternalEvents.BasketDeletedFailed,
            new BasketDeletedFailedIntegrationEvent(
                orderId,
                basketId,
                "failed@example.com",
                120.00m
            )
        );

        var result = await runTask;

        result.FinalStatus.ShouldBe(OrderOrchestrationStatus.BasketDeletionFailed);
        result.Email.ShouldBe("failed@example.com");
        result.TotalMoney.ShouldBe(120.00m);

        harness
            .CalledActivities.Select(a => a.Name)
            .ShouldBe([
                nameof(PublishPlaceOrderActivity),
                nameof(PublishDeleteBasketFailedActivity),
            ]);

        var failedCmd = harness
            .CalledActivities[1]
            .Input.ShouldBeOfType<DeleteBasketFailedCommand>();
        failedCmd.OrderId.ShouldBe(orderId);
        failedCmd.BasketId.ShouldBe(basketId);
        failedCmd.Email.ShouldBe("failed@example.com");
        failedCmd.TotalMoney.ShouldBe(120.00m);
    }

    [Test]
    public async Task GivenOrderCancelled_WhenEventReceived_ThenShouldPublishCancelCommandAndFinalize()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var input = CreateInput(orderId, basketId);
        var harness = new OrchestratorTestHarness(input);

        var runTask = new OrderOrchestrator().RunAsync(harness.Context.Object, input);

        harness.CompleteExternalEvent(
            OrderOrchestrationExternalEvents.OrderCancelled,
            new OrderStatusChangedToCancelIntegrationEvent(
                orderId,
                basketId,
                "Bob Doe",
                "bob@example.com",
                180.00m
            )
        );

        var result = await runTask;

        result.FinalStatus.ShouldBe(OrderOrchestrationStatus.Cancelled);
        result.FullName.ShouldBe("Bob Doe");
        result.Email.ShouldBe("bob@example.com");
        result.TotalMoney.ShouldBe(180.00m);

        harness
            .CalledActivities.Select(a => a.Name)
            .ShouldBe([nameof(PublishPlaceOrderActivity), nameof(PublishCancelOrderActivity)]);
    }

    [Test]
    public async Task GivenTimeouts_WhenMaxAttemptsExceeded_ThenShouldRetryAndCancel()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var input = CreateInput(orderId, basketId, maxAttempts: 3);
        var harness = new OrchestratorTestHarness(input);

        var runTask = new OrderOrchestrator().RunAsync(harness.Context.Object, input);

        // Fire 3 consecutive timeouts (2 retries + final cancellation)
        for (var i = 0; i < 3; i++)
        {
            await harness.FireNextTimerAsync(runTask);
        }

        var result = await runTask;

        result.FinalStatus.ShouldBe(OrderOrchestrationStatus.Cancelled);
        result.TimeoutRetryCount.ShouldBe(3);

        harness
            .CalledActivities.Select(a => a.Name)
            .ShouldBe([
                nameof(PublishPlaceOrderActivity),
                nameof(PublishPlaceOrderActivity),
                nameof(PublishPlaceOrderActivity),
                nameof(PublishCancelOrderActivity),
            ]);
    }

    [Test]
    public async Task GivenMissingNotificationDetails_WhenOrderCompleted_ThenShouldSkipNotificationCommand()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var input = CreateInput(orderId, basketId, null, null);
        var harness = new OrchestratorTestHarness(input);

        var runTask = new OrderOrchestrator().RunAsync(harness.Context.Object, input);

        harness.CompleteExternalEvent(
            OrderOrchestrationExternalEvents.OrderCompleted,
            new OrderStatusChangedToCompleteIntegrationEvent(orderId, basketId, null, null, 50.00m)
        );

        var result = await runTask;

        result.FinalStatus.ShouldBe(OrderOrchestrationStatus.Completed);
        harness.CalledActivities.Select(a => a.Name).ShouldBe([nameof(PublishPlaceOrderActivity)]);
    }

    [Test]
    public async Task GivenSameInputs_WhenReplayed_ThenShouldProduceDeterministicResultAndCommands()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var input = CreateInput(orderId, basketId);

        async Task<(OrderOrchestrationResult Result, string[] Activities)> ExecuteAsync()
        {
            var harness = new OrchestratorTestHarness(input);
            var task = new OrderOrchestrator().RunAsync(harness.Context.Object, input);
            harness.CompleteExternalEvent(
                OrderOrchestrationExternalEvents.BasketDeletedComplete,
                new BasketDeletedCompleteIntegrationEvent(orderId, basketId, 99.99m)
            );
            await harness.WaitForActivityCountAsync(2);
            harness.CompleteExternalEvent(
                OrderOrchestrationExternalEvents.OrderCompleted,
                new OrderStatusChangedToCompleteIntegrationEvent(
                    orderId,
                    basketId,
                    "John Doe",
                    "john@example.com",
                    99.99m
                )
            );
            var res = await task;
            return (res, harness.CalledActivities.Select(a => a.Name).ToArray());
        }

        var (firstResult, firstActivities) = await ExecuteAsync();
        var (secondResult, secondActivities) = await ExecuteAsync();

        secondResult.ShouldBe(firstResult);
        secondActivities.ShouldBe(firstActivities);
    }

    [Test]
    public async Task GivenActivityFailureAfterBasketDeleted_WhenTaskFailedExceptionThrown_ThenShouldCompensateInLifoOrderAndFail()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var input = CreateInput(orderId, basketId);
        var harness = new OrchestratorTestHarness(input);

        harness.FailActivity(
            nameof(PublishDeleteBasketCompleteActivity),
            new TaskFailedException(
                nameof(PublishDeleteBasketCompleteActivity),
                1,
                new InvalidOperationException("Kafka broker unavailable")
            )
        );

        var runTask = new OrderOrchestrator().RunAsync(harness.Context.Object, input);

        harness.CompleteExternalEvent(
            OrderOrchestrationExternalEvents.BasketDeletedComplete,
            new BasketDeletedCompleteIntegrationEvent(orderId, basketId, 99.99m)
        );

        var result = await runTask;

        result.FinalStatus.ShouldBe(OrderOrchestrationStatus.Failed);
        result.FailureReason.ShouldNotBeNullOrWhiteSpace();
        harness
            .CalledActivities.Select(a => a.Name)
            .ShouldBe([
                nameof(PublishPlaceOrderActivity),
                nameof(PublishDeleteBasketCompleteActivity),
                nameof(PublishDeleteBasketFailedActivity),
                nameof(PublishCancelOrderActivity),
            ]);
    }

    [Test]
    public async Task GivenCompensationFailure_WhenCompensatingInLifoOrder_ThenShouldContinueRemainingCompensations()
    {
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var input = CreateInput(orderId, basketId);
        var harness = new OrchestratorTestHarness(input);

        harness.FailActivity(
            nameof(PublishDeleteBasketCompleteActivity),
            new TaskFailedException(
                nameof(PublishDeleteBasketCompleteActivity),
                1,
                new InvalidOperationException("Downstream failure")
            )
        );
        harness.FailActivity(
            nameof(PublishDeleteBasketFailedActivity),
            new InvalidOperationException("Compensation failure")
        );

        var runTask = new OrderOrchestrator().RunAsync(harness.Context.Object, input);

        harness.CompleteExternalEvent(
            OrderOrchestrationExternalEvents.BasketDeletedComplete,
            new BasketDeletedCompleteIntegrationEvent(orderId, basketId, 99.99m)
        );

        var result = await runTask;

        result.FinalStatus.ShouldBe(OrderOrchestrationStatus.Failed);
        harness
            .CalledActivities.Select(a => a.Name)
            .ShouldBe([
                nameof(PublishPlaceOrderActivity),
                nameof(PublishDeleteBasketCompleteActivity),
                nameof(PublishDeleteBasketFailedActivity),
                nameof(PublishCancelOrderActivity),
            ]);
    }

    private static OrderOrchestrationInput CreateInput(
        Guid orderId,
        Guid basketId,
        string? fullName = "John Doe",
        string? email = "john@example.com",
        int maxAttempts = 3
    )
    {
        return new(
            orderId,
            basketId,
            fullName,
            email,
            99.99m,
            maxAttempts,
            TimeSpan.FromMinutes(30)
        );
    }

    private sealed class OrchestratorTestHarness
    {
        private readonly Dictionary<string, TaskCompletionSource<object>> _externalEvents = [];
        private readonly Dictionary<string, Exception> _failingActivities = [];
        private readonly Queue<TaskCompletionSource> _timers = [];

        public OrchestratorTestHarness(OrderOrchestrationInput input)
        {
            Context.Setup(c => c.InstanceId).Returns(input.OrderId.ToString());
            Context.Setup(c => c.CurrentUtcDateTime).Returns(_fixedUtcNow);
            Context.Setup(c => c.GetInput<OrderOrchestrationInput>()).Returns(input);
            Context
                .Setup(c => c.CreateReplaySafeLogger<OrderOrchestrator>())
                .Returns(NullLogger<OrderOrchestrator>.Instance);
            Context
                .Setup(c => c.CreateReplaySafeLogger<OrderSagaCompensations>())
                .Returns(NullLogger<OrderSagaCompensations>.Instance);

            Context
                .Setup(c =>
                    c.CallActivityAsync(
                        It.IsAny<TaskName>(),
                        It.IsAny<object?>(),
                        It.IsAny<TaskOptions?>()
                    )
                )
                .Returns<TaskName, object?, TaskOptions?>(
                    (name, payload, _) =>
                    {
                        CalledActivities.Add((name.Name, payload));
                        if (_failingActivities.TryGetValue(name.Name, out var ex))
                        {
                            return Task.FromException(ex);
                        }

                        return Task.CompletedTask;
                    }
                );

            SetupExternalEvent<BasketDeletedCompleteIntegrationEvent>(
                OrderOrchestrationExternalEvents.BasketDeletedComplete
            );
            SetupExternalEvent<BasketDeletedFailedIntegrationEvent>(
                OrderOrchestrationExternalEvents.BasketDeletedFailed
            );
            SetupExternalEvent<OrderStatusChangedToCompleteIntegrationEvent>(
                OrderOrchestrationExternalEvents.OrderCompleted
            );
            SetupExternalEvent<OrderStatusChangedToCancelIntegrationEvent>(
                OrderOrchestrationExternalEvents.OrderCancelled
            );

            Context
                .Setup(c => c.CreateTimer(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns<DateTime, CancellationToken>((_, ct) => CreateTimerTask(ct));
            Context
                .Setup(c => c.CreateTimer(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns<TimeSpan, CancellationToken>((_, ct) => CreateTimerTask(ct));
        }

        public Mock<TaskOrchestrationContext> Context { get; } = new();
        public List<(string Name, object? Input)> CalledActivities { get; } = [];

        private Task CreateTimerTask(CancellationToken ct)
        {
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            ct.Register(() => tcs.TrySetCanceled(ct));
            _timers.Enqueue(tcs);
            return tcs.Task;
        }

        public void FailActivity(string activityName, Exception exception)
        {
            _failingActivities[activityName] = exception;
        }

        public void CompleteExternalEvent<T>(string eventName, T payload)
            where T : notnull
        {
            _externalEvents[eventName].TrySetResult(payload);
        }

        public async Task WaitForActivityCountAsync(int count)
        {
            var deadline = DateTime.UtcNow.AddSeconds(5);
            while (CalledActivities.Count < count)
            {
                if (DateTime.UtcNow > deadline)
                {
                    throw new TimeoutException($"Timed out waiting for {count} activities.");
                }

                await Task.Yield();
            }
        }

        public async Task FireNextTimerAsync(Task? orchestrationTask = null)
        {
            var deadline = DateTime.UtcNow.AddSeconds(5);
            while (_timers.Count == 0)
            {
                if (orchestrationTask?.IsCompleted == true)
                {
                    await orchestrationTask;
                    throw new InvalidOperationException(
                        "Orchestration completed before creating the expected timer."
                    );
                }

                if (DateTime.UtcNow > deadline)
                {
                    throw new TimeoutException("Timed out waiting for orchestration timer.");
                }

                await Task.Yield();
            }

            _timers.Dequeue().TrySetResult();
            await Task.Yield();
        }

        private void SetupExternalEvent<T>(string eventName)
        {
            var tcs = new TaskCompletionSource<object>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            _externalEvents[eventName] = tcs;

            Context
                .Setup(c => c.WaitForExternalEvent<T>(eventName, It.IsAny<CancellationToken>()))
                .Returns(async () => (T)await tcs.Task);
        }
    }
}
