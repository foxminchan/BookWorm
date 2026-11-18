using BookWorm.Contracts;
using BookWorm.Scheduler.Jobs;
using Microsoft.Extensions.Logging;
using Quartz;
using Wolverine;

namespace BookWorm.Scheduler.UnitTests;

public sealed class ProcessVectorBatchJobTests
{
    [Test]
    public async Task GivenValidDependencies_WhenExecutingJob_ThenShouldPublishExactlyOneEvent()
    {
        var busMock = new Mock<IMessageBus>();
        var logger = Mock.Of<ILogger<ProcessVectorBatchJob>>();
        var job = new ProcessVectorBatchJob(busMock.Object, logger);
        var context = Mock.Of<IJobExecutionContext>(c =>
            c.CancellationToken == CancellationToken.None
        );

        await job.Execute(context);

        busMock.Verify(
            x =>
                x.PublishAsync(
                    It.IsAny<ProcessVectorBatchIntegrationEvent>(),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenPublishThrows_WhenExecutingJob_ThenShouldWrapInJobExecutionException()
    {
        var busMock = new Mock<IMessageBus>();
        var logger = Mock.Of<ILogger<ProcessVectorBatchJob>>();
        var job = new ProcessVectorBatchJob(busMock.Object, logger);
        var context = Mock.Of<IJobExecutionContext>(c =>
            c.CancellationToken == CancellationToken.None
        );

        busMock
            .Setup(x =>
                x.PublishAsync(
                    It.IsAny<ProcessVectorBatchIntegrationEvent>(),
                    It.IsAny<DeliveryOptions>()
                )
            )
            .ThrowsAsync(new InvalidOperationException("Bus failure"));

        var exception = await Should.ThrowAsync<JobExecutionException>(() =>
            job.Execute(context).AsTask()
        );

        exception.InnerException.ShouldBeOfType<InvalidOperationException>();
        exception.InnerException?.Message.ShouldBe("Bus failure");
    }

    [Test]
    public async Task GivenCancellation_WhenExecutingJob_ThenShouldNotPublishEvent()
    {
        var busMock = new Mock<IMessageBus>();
        var job = new ProcessVectorBatchJob(
            busMock.Object,
            Mock.Of<ILogger<ProcessVectorBatchJob>>()
        );
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() =>
            job.Execute(Mock.Of<IJobExecutionContext>(), cancellation.Token).AsTask()
        );
        busMock.VerifyNoOtherCalls();
    }
}
