using BookWorm.Contracts;
using BookWorm.Scheduler.Jobs;
using Microsoft.Extensions.Logging;
using Quartz;
using Wolverine;

namespace BookWorm.Scheduler.UnitTests;

public sealed class ResendErrorEmailJobTests
{
    [Test]
    public async Task GivenValidDependencies_WhenExecutingJob_ThenShouldPublishExactlyOneEvent()
    {
        // Arrange
        var messageBusMock = new Mock<IMessageBus>();
        var logger = Mock.Of<ILogger<ResendErrorEmailJob>>();
        var job = new ResendErrorEmailJob(messageBusMock.Object, logger);
        var context = Mock.Of<IJobExecutionContext>(c =>
            c.CancellationToken == CancellationToken.None
        );

        // Act
        await job.Execute(context);

        // Assert
        messageBusMock.Verify(
            x => x.PublishAsync(It.IsAny<ResendErrorEmailIntegrationEvent>(), null),
            Times.Once
        );
    }

    [Test]
    public async Task GivenMultipleExecutions_WhenExecutingJobConcurrently_ThenShouldHandleEachExecutionIndependently()
    {
        // Arrange
        var messageBusMock = new Mock<IMessageBus>();
        var logger = Mock.Of<ILogger<ResendErrorEmailJob>>();
        var job = new ResendErrorEmailJob(messageBusMock.Object, logger);
        var context = Mock.Of<IJobExecutionContext>(c =>
            c.CancellationToken == CancellationToken.None
        );
        const int numberOfExecutions = 3;

        // Act
        var tasks = Enumerable
            .Range(0, numberOfExecutions)
            .Select(_ => job.Execute(context))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        messageBusMock.Verify(
            x => x.PublishAsync(It.IsAny<ResendErrorEmailIntegrationEvent>(), null),
            Times.Exactly(numberOfExecutions)
        );
    }

    [Test]
    public async Task GivenPublishThrows_WhenExecutingJob_ThenShouldWrapInJobExecutionException()
    {
        // Arrange
        var messageBusMock = new Mock<IMessageBus>();
        var loggerMock = new Mock<ILogger<ResendErrorEmailJob>>();
        var job = new ResendErrorEmailJob(messageBusMock.Object, loggerMock.Object);
        var context = Mock.Of<IJobExecutionContext>(c =>
            c.CancellationToken == CancellationToken.None
        );

        messageBusMock
            .Setup(x => x.PublishAsync(It.IsAny<ResendErrorEmailIntegrationEvent>(), null))
            .ThrowsAsync(new InvalidOperationException("Bus failure"));

        // Act
        var exception = await Should.ThrowAsync<JobExecutionException>(() => job.Execute(context));

        // Assert
        exception.InnerException.ShouldBeOfType<InvalidOperationException>();
        exception.InnerException!.Message.ShouldBe("Bus failure");
    }
}
