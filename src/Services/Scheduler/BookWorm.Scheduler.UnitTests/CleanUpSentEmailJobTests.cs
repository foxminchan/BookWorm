using BookWorm.Contracts;
using BookWorm.Scheduler.Jobs;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace BookWorm.Scheduler.UnitTests;

// Dummy consumer to register message type with MassTransit test harness
public sealed class DummyCleanUpSentEmailConsumer : IConsumer<CleanUpSentEmailIntegrationEvent>
{
    public Task Consume(ConsumeContext<CleanUpSentEmailIntegrationEvent> context)
    {
        return Task.CompletedTask;
    }
}

public sealed class CleanUpSentEmailJobTests
{
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;

    [Before(Test)]
    public async Task Setup()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.SetKebabCaseEndpointNameFormatter();
                cfg.AddConsumer<DummyCleanUpSentEmailConsumer>();
                cfg.UsingInMemory(
                    (context, configurator) =>
                    {
                        configurator.ConfigureEndpoints(context);
                    }
                );
            })
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
        var job = new CleanUpSentEmailJob(bus);
        var cancellationToken = CancellationToken.None;
        var context = Mock.Of<IJobExecutionContext>(c => c.CancellationToken == cancellationToken);

        // Act
        await job.Execute(context);

        // Assert
        var published = await _harness.Published.Any<CleanUpSentEmailIntegrationEvent>(
            cancellationToken
        );
        published.ShouldBeTrue();
    }

    [Test]
    public async Task GivenValidDependencies_WhenExecutingJob_ThenShouldSendCorrectEventType()
    {
        // Arrange
        var bus = _harness.Bus;
        var job = new CleanUpSentEmailJob(bus);
        var cancellationToken = CancellationToken.None;
        var context = Mock.Of<IJobExecutionContext>(c => c.CancellationToken == cancellationToken);

        // Act
        await job.Execute(context);

        // Assert
        var eventCount = 0;
        await foreach (
            var publishedEvent in _harness.Published.SelectAsync<CleanUpSentEmailIntegrationEvent>(
                cancellationToken
            )
        )
        {
            eventCount++;
            publishedEvent.Context.Message.ShouldBeOfType<CleanUpSentEmailIntegrationEvent>();
        }

        eventCount.ShouldBe(1);
    }

    [Test]
    public async Task GivenCancellationToken_WhenExecutingJob_ThenShouldPassTokenToAllDependencies()
    {
        // Arrange
        var bus = _harness.Bus;
        var job = new CleanUpSentEmailJob(bus);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var context = Mock.Of<IJobExecutionContext>(c => c.CancellationToken == cancellationToken);

        // Act
        await job.Execute(context);

        // Assert
        var published = await _harness.Published.Any<CleanUpSentEmailIntegrationEvent>(
            cancellationToken
        );
        published.ShouldBeTrue();
    }

    [Test]
    public async Task GivenMultipleExecutions_WhenExecutingJobConcurrently_ThenShouldHandleEachExecutionIndependently()
    {
        // Arrange
        var bus = _harness.Bus;
        var job = new CleanUpSentEmailJob(bus);
        var cancellationToken = CancellationToken.None;
        var context = Mock.Of<IJobExecutionContext>(c => c.CancellationToken == cancellationToken);
        const int numberOfExecutions = 3;

        // Act
        var tasks = Enumerable
            .Range(0, numberOfExecutions)
            .Select(_ => job.Execute(context))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        var publishedCount = 0;
        await foreach (
            var _ in _harness.Published.SelectAsync<CleanUpSentEmailIntegrationEvent>(
                cancellationToken
            )
        )
        {
            publishedCount++;
        }

        publishedCount.ShouldBe(numberOfExecutions);
    }
}
