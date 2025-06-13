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
    private readonly Mock<IEventStoreOperations> _eventStoreMock = new();
    private readonly Guid _orderId = Guid.CreateVersion7();

    [Test]
    public async Task GivenValidCommand_WhenHandlingTaskCompletes_ThenShouldReturnCompletedTask()
    {
        // Arrange
        var command = new DeleteBasketCompleteCommand(_orderId, TotalMoney);

        // Set up the mock to return a completed task
        _documentSessionMock.Setup(x => x.Events).Returns(_eventStoreMock.Object);
        _documentSessionMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketCompleteCommandHandler>())
            .AddScoped(_ => _documentSessionMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<DeleteBasketCompleteCommandHandler>();
        (await consumerHarness.Consumed.Any<DeleteBasketCompleteCommand>()).ShouldBeTrue();

        // No fault should be published
        (await harness.Published.Any<Fault<DeleteBasketCompleteCommand>>()).ShouldBeFalse();

        // Verify the StartStream method was called
        _eventStoreMock.Verify(
            x =>
                x.StartStream<OrderSummary>(
                    It.IsAny<Guid>(),
                    It.IsAny<DeleteBasketCompleteCommand>()
                ),
            Times.Once
        );

        // Verify SaveChangesAsync was called
        _documentSessionMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );

        await harness.Stop();
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingTaskFaults_ThenShouldPublishFault()
    {
        // Arrange
        var command = new DeleteBasketCompleteCommand(_orderId, TotalMoney);
        var expectedException = new InvalidOperationException("Task faulted");

        // Set up the mock to return a faulted task
        _documentSessionMock.Setup(x => x.Events).Returns(_eventStoreMock.Object);
        _documentSessionMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromException(expectedException));

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketCompleteCommandHandler>())
            .AddScoped(_ => _documentSessionMock.Object)
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
        using var cancellationTokenSource = new CancellationTokenSource();

        // Cancel the token before using it in the mock
        await cancellationTokenSource.CancelAsync();

        // Set up the mock to return a canceled task
        _documentSessionMock.Setup(x => x.Events).Returns(_eventStoreMock.Object);
        _documentSessionMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromCanceled(cancellationTokenSource.Token));

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketCompleteCommandHandler>())
            .AddScoped(_ => _documentSessionMock.Object)
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

        await harness.Stop(cancellationTokenSource.Token);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingDelayedTask_ThenShouldEventuallyComplete()
    {
        // Arrange
        var command = new DeleteBasketCompleteCommand(_orderId, TotalMoney);
        var delayTime = TimeSpan.FromMilliseconds(50);

        // Set up the mock to return a delayed task
        _documentSessionMock.Setup(x => x.Events).Returns(_eventStoreMock.Object);
        _documentSessionMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.Delay(delayTime));

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketCompleteCommandHandler>())
            .AddScoped(_ => _documentSessionMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Wait a bit longer than the delay to ensure the task completes
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
