using BookWorm.Contracts;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.IntegrationEvents.EventHandlers;
using Marten;
using Marten.Events;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Ordering.UnitTests.Consumers;

public sealed class DeleteBasketCompleteConsumerTests
{
    private const decimal TotalMoney = 125.99m;
    private readonly Mock<IDocumentSession> _documentSessionMock = new();
    private readonly Guid _orderId = Guid.CreateVersion7();

    [Test]
    public async Task GivenValidCommand_WhenHandlingTaskCompletes_ThenShouldReturnCompletedTask()
    {
        // Arrange
        var command = new DeleteBasketCompleteCommand(_orderId, TotalMoney);
        var taskCompletionSource = new TaskCompletionSource<bool>();

        // Use a simpler mocking approach to avoid the lambda expression issue
        _documentSessionMock
            .Setup(x => x.Events)
            .Returns(
                Mock.Of<IEventStore>(store =>
                    store.WriteToAggregate(
                        It.IsAny<Guid>(),
                        It.IsAny<Action<IEventStream<OrderSummary>>>(),
                        It.IsAny<CancellationToken>()
                    ) == Task.FromResult(taskCompletionSource.Task)
                )
            );

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketCompleteCommandHandler>())
            .AddScoped<IDocumentSession>(_ => _documentSessionMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Complete the task before triggering the handler
        taskCompletionSource.SetResult(true);

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<DeleteBasketCompleteCommandHandler>();
        (await consumerHarness.Consumed.Any<DeleteBasketCompleteCommand>()).ShouldBeTrue();

        // No fault should be published
        (await harness.Published.Any<Fault<DeleteBasketCompleteCommand>>()).ShouldBeFalse();

        await harness.Stop();
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingTaskFaults_ThenShouldPublishFault()
    {
        // Arrange
        var command = new DeleteBasketCompleteCommand(_orderId, TotalMoney);
        var expectedException = new InvalidOperationException("Task faulted");

        // Set up the mock to return a faulted task
        _documentSessionMock
            .Setup(x => x.Events)
            .Returns(
                Mock.Of<IEventStore>(store =>
                    store.WriteToAggregate(
                        It.IsAny<Guid>(),
                        It.IsAny<Action<IEventStream<OrderSummary>>>(),
                        It.IsAny<CancellationToken>()
                    ) == Task.FromException(expectedException)
                )
            );

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketCompleteCommandHandler>())
            .AddScoped<IDocumentSession>(_ => _documentSessionMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<DeleteBasketCompleteCommandHandler>();

        // Command should be consumed
        (await consumerHarness.Consumed.Any<DeleteBasketCompleteCommand>()).ShouldBeTrue();

        // Fault should be published
        (await harness.Published.Any<Fault<DeleteBasketCompleteCommand>>()).ShouldBeTrue();

        await harness.Stop();
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingTaskCancelled_ThenShouldPublishFault()
    {
        // Arrange
        var command = new DeleteBasketCompleteCommand(_orderId, TotalMoney);
        var cancellationTokenSource = new CancellationTokenSource();

        // Cancel the token before using it in the mock
        await cancellationTokenSource.CancelAsync();

        // Set up the mock to return a canceled task
        _documentSessionMock
            .Setup(x => x.Events)
            .Returns(
                Mock.Of<IEventStore>(store =>
                    store.WriteToAggregate(
                        It.IsAny<Guid>(),
                        It.IsAny<Action<IEventStream<OrderSummary>>>(),
                        It.IsAny<CancellationToken>()
                    ) == Task.FromCanceled<bool>(cancellationTokenSource.Token)
                )
            );

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketCompleteCommandHandler>())
            .AddScoped<IDocumentSession>(_ => _documentSessionMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command, CancellationToken.None);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<DeleteBasketCompleteCommandHandler>();

        // Command should be consumed
        (
            await consumerHarness.Consumed.Any<DeleteBasketCompleteCommand>(CancellationToken.None)
        ).ShouldBeTrue();

        // Fault should be published due to cancellation
        (
            await harness.Published.Any<Fault<DeleteBasketCompleteCommand>>(CancellationToken.None)
        ).ShouldBeTrue();

        await harness.Stop(cancellationToken: cancellationTokenSource.Token);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingDelayedTask_ThenShouldEventuallyComplete()
    {
        // Arrange
        var command = new DeleteBasketCompleteCommand(_orderId, TotalMoney);
        var delayTime = TimeSpan.FromMilliseconds(50);

        // Set up the mock to return a delayed task
        _documentSessionMock
            .Setup(x => x.Events)
            .Returns(
                Mock.Of<IEventStore>(store =>
                    store.WriteToAggregate(
                        It.IsAny<Guid>(),
                        It.IsAny<Action<IEventStream<OrderSummary>>>(),
                        It.IsAny<CancellationToken>()
                    ) == Task.Delay(delayTime)
                )
            );

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketCompleteCommandHandler>())
            .AddScoped<IDocumentSession>(_ => _documentSessionMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Wait a bit longer than the delay to ensure task completes
        await Task.Delay(delayTime.Add(TimeSpan.FromMilliseconds(50)));

        // Assert
        var consumerHarness = harness.GetConsumerHarness<DeleteBasketCompleteCommandHandler>();

        // Command should be consumed
        (await consumerHarness.Consumed.Any<DeleteBasketCompleteCommand>()).ShouldBeTrue();

        // No fault should be published
        (await harness.Published.Any<Fault<DeleteBasketCompleteCommand>>()).ShouldBeFalse();

        await harness.Stop();
    }
}
