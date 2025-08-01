using BookWorm.Contracts;
using BookWorm.Scheduler.Infrastructure;
using BookWorm.Scheduler.Jobs;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Scheduler.UnitTests;

public sealed class DummyResendErrorEmailConsumer : IConsumer<ResendErrorEmailIntegrationEvent>
{
    public Task Consume(ConsumeContext<ResendErrorEmailIntegrationEvent> context)
    {
        return Task.CompletedTask;
    }
}

public sealed class ResendErrorEmailJobTests
{
    private readonly Mock<ISchedulerDbContext> _dbContextMock = new();
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;

    [Before(Test)]
    public async Task Setup()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.SetKebabCaseEndpointNameFormatter();
                cfg.AddConsumer<DummyResendErrorEmailConsumer>();
                cfg.UsingInMemory(
                    (context, configurator) =>
                    {
                        configurator.ConfigureEndpoints(context);
                    }
                );
            })
            .AddScoped(_ => _dbContextMock.Object)
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    [After(Test)]
    public async Task TearDown()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Test]
    public async Task GivenValidDependencies_WhenExecutingJob_ThenShouldSendEventAndSaveChanges()
    {
        // Arrange
        var bus = _harness.Bus;
        var job = new ResendErrorEmailJob(bus, _dbContextMock.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        await job.ExecuteAsync(cancellationToken);

        // Assert
        var published = await _harness.Published.Any<ResendErrorEmailIntegrationEvent>(
            cancellationToken
        );
        published.ShouldBeTrue();

        _dbContextMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenValidDependencies_WhenExecutingJob_ThenShouldSendCorrectEventType()
    {
        // Arrange
        var bus = _harness.Bus;
        var job = new ResendErrorEmailJob(bus, _dbContextMock.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        await job.ExecuteAsync(cancellationToken);

        // Assert
        var eventCount = 0;
        await foreach (
            var publishedEvent in _harness.Published.SelectAsync<ResendErrorEmailIntegrationEvent>(
                cancellationToken
            )
        )
        {
            eventCount++;
            publishedEvent.Context.Message.ShouldBeOfType<ResendErrorEmailIntegrationEvent>();
        }

        eventCount.ShouldBe(1);
    }

    [Test]
    public async Task GivenBusThrowsException_WhenExecutingJob_ThenShouldPropagateException()
    {
        // Arrange
        var dbContextThrowingMock = new Mock<ISchedulerDbContext>();
        var bus = _harness.Bus;
        var job = new ResendErrorEmailJob(bus, dbContextThrowingMock.Object);
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Database save failed");

        dbContextThrowingMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var thrownException = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await job.ExecuteAsync(cancellationToken)
        );

        thrownException.Message.ShouldBe("Database save failed");

        var published = await _harness.Published.Any<ResendErrorEmailIntegrationEvent>(
            cancellationToken
        );
        published.ShouldBeTrue();

        dbContextThrowingMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenDbContextThrowsException_WhenExecutingJob_ThenShouldPropagateException()
    {
        // Arrange
        var bus = _harness.Bus;
        var job = new ResendErrorEmailJob(bus, _dbContextMock.Object);
        var cancellationToken = CancellationToken.None;
        var expectedException = new ArgumentException("Invalid database operation");

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var thrownException = await Should.ThrowAsync<ArgumentException>(async () =>
            await job.ExecuteAsync(cancellationToken)
        );

        thrownException.Message.ShouldBe("Invalid database operation");

        var published = await _harness.Published.Any<ResendErrorEmailIntegrationEvent>(
            cancellationToken
        );
        published.ShouldBeTrue();

        _dbContextMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenCancellationToken_WhenExecutingJob_ThenShouldPassTokenToAllDependencies()
    {
        // Arrange
        var bus = _harness.Bus;
        var job = new ResendErrorEmailJob(bus, _dbContextMock.Object);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Act
        await job.ExecuteAsync(cancellationToken);

        // Assert
        var published = await _harness.Published.Any<ResendErrorEmailIntegrationEvent>(
            cancellationToken
        );
        published.ShouldBeTrue();

        _dbContextMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenCancelledToken_WhenExecutingJob_ThenShouldThrowOperationCancelledException()
    {
        // Arrange
        var bus = _harness.Bus;
        var job = new ResendErrorEmailJob(bus, _dbContextMock.Object);
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var cancellationToken = cancellationTokenSource.Token;

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await job.ExecuteAsync(cancellationToken)
        );
    }

    [Test]
    public async Task GivenSuccessfulExecution_WhenExecutingJob_ThenShouldExecuteInCorrectOrder()
    {
        // Arrange
        var bus = _harness.Bus;
        var job = new ResendErrorEmailJob(bus, _dbContextMock.Object);
        var cancellationToken = CancellationToken.None;
        var executionOrder = new List<string>();

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .Callback(() => executionOrder.Add("SaveChanges"));

        // Act
        await job.ExecuteAsync(cancellationToken);

        // Assert
        var published = await _harness.Published.Any<ResendErrorEmailIntegrationEvent>(
            cancellationToken
        );
        published.ShouldBeTrue();

        executionOrder.Count.ShouldBe(1);
        executionOrder[0].ShouldBe("SaveChanges");
    }

    [Test]
    public async Task GivenMultipleExecutions_WhenExecutingJobConcurrently_ThenShouldHandleEachExecutionIndependently()
    {
        // Arrange
        var bus = _harness.Bus;
        var job = new ResendErrorEmailJob(bus, _dbContextMock.Object);
        var cancellationToken = CancellationToken.None;
        const int numberOfExecutions = 3;

        // Act
        var tasks = Enumerable
            .Range(0, numberOfExecutions)
            .Select(_ => job.ExecuteAsync(cancellationToken))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        var publishedCount = 0;
        await foreach (
            var _ in _harness.Published.SelectAsync<ResendErrorEmailIntegrationEvent>(
                cancellationToken
            )
        )
        {
            publishedCount++;
        }

        publishedCount.ShouldBe(numberOfExecutions);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Exactly(numberOfExecutions)
        );
    }

    [Test]
    public void GivenResendErrorEmailJob_WhenCreated_ThenShouldNotBeNull()
    {
        // Arrange
        var bus = _harness.Bus;

        // Act
        var job = new ResendErrorEmailJob(bus, _dbContextMock.Object);

        // Assert
        job.ShouldNotBeNull();
    }
}
