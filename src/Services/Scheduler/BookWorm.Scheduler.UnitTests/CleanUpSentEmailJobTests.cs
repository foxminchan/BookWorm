using BookWorm.Chassis.EventBus.Serialization;
using BookWorm.Contracts;
using BookWorm.Scheduler.Jobs;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BookWorm.Scheduler.UnitTests;

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
                        configurator.UseCloudEvents();
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
    public async Task GivenValidDependencies_WhenExecutingJob_ThenShouldPublishExactlyOneEvent()
    {
        // Arrange
        var bus = _harness.Bus;
        var logger = Mock.Of<ILogger<CleanUpSentEmailJob>>();
        var job = new CleanUpSentEmailJob(bus, logger);
        var context = Mock.Of<IJobExecutionContext>(c =>
            c.CancellationToken == CancellationToken.None
        );

        // Act
        await job.Execute(context);

        // Assert
        var publishedCount = 0;
        await foreach (var _ in _harness.Published.SelectAsync<CleanUpSentEmailIntegrationEvent>())
        {
            publishedCount++;
        }

        publishedCount.ShouldBe(1);
    }

    [Test]
    public async Task GivenMultipleExecutions_WhenExecutingJobConcurrently_ThenShouldHandleEachExecutionIndependently()
    {
        // Arrange
        var bus = _harness.Bus;
        var logger = Mock.Of<ILogger<CleanUpSentEmailJob>>();
        var job = new CleanUpSentEmailJob(bus, logger);
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

    [Test]
    public async Task GivenPublishThrows_WhenExecutingJob_ThenShouldWrapInJobExecutionException()
    {
        // Arrange
        var busMock = new Mock<IBus>();
        var loggerMock = new Mock<ILogger<CleanUpSentEmailJob>>();
        var job = new CleanUpSentEmailJob(busMock.Object, loggerMock.Object);
        var context = Mock.Of<IJobExecutionContext>(c =>
            c.CancellationToken == CancellationToken.None
        );

        busMock
            .Setup(x =>
                x.Publish(
                    It.IsAny<CleanUpSentEmailIntegrationEvent>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new InvalidOperationException("Bus failure"));

        // Act
        var exception = await Should.ThrowAsync<JobExecutionException>(() => job.Execute(context));

        // Assert
        exception.InnerException.ShouldBeOfType<InvalidOperationException>();
        exception.InnerException!.Message.ShouldBe("Bus failure");
    }
}
