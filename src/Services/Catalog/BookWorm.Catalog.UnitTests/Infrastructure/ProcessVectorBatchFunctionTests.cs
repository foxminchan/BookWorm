using BookWorm.Catalog.Ingestion;
using Microsoft.Extensions.Logging;

namespace BookWorm.Catalog.UnitTests.Infrastructure;

public sealed class ProcessVectorBatchFunctionTests
{
    [Test]
    public async Task GivenKafkaEvent_WhenInvoked_ThenShouldProcessOneVectorBatch()
    {
        using var cancellation = new CancellationTokenSource();
        var cancellationToken = cancellation.Token;
        var processor = new Mock<IVectorBatchProcessor>();
        processor.Setup(x => x.ProcessAsync(cancellationToken)).ReturnsAsync(7);
        var function = new ProcessVectorBatchFunction(
            processor.Object,
            Mock.Of<ILogger<ProcessVectorBatchFunction>>()
        );

        await function.ProcessVectorBatch("{}", cancellationToken);

        processor.Verify(x => x.ProcessAsync(cancellationToken), Times.Once);
        processor.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenProcessingFails_WhenInvoked_ThenShouldSurfaceFailure()
    {
        var processor = new Mock<IVectorBatchProcessor>();
        processor
            .Setup(x => x.ProcessAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Embedding service unavailable"));
        var function = new ProcessVectorBatchFunction(
            processor.Object,
            Mock.Of<ILogger<ProcessVectorBatchFunction>>()
        );

        await Should.ThrowAsync<InvalidOperationException>(() =>
            function.ProcessVectorBatch("{}", CancellationToken.None)
        );
    }

    [Test]
    public async Task GivenProcessingCancelled_WhenInvoked_ThenShouldPropagateCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var cancellationToken = cancellation.Token;
        var processor = new Mock<IVectorBatchProcessor>();
        processor
            .Setup(x => x.ProcessAsync(cancellationToken))
            .Returns(Task.FromCanceled<int>(cancellationToken));
        var function = new ProcessVectorBatchFunction(
            processor.Object,
            Mock.Of<ILogger<ProcessVectorBatchFunction>>()
        );

        await Should.ThrowAsync<OperationCanceledException>(() =>
            function.ProcessVectorBatch("{}", cancellationToken)
        );
    }

    [Test]
    public async Task GivenCloudEventsEnvelope_WhenInvoked_ThenShouldDeserializeAndProcessVectorBatch()
    {
        var eventId = Guid.CreateVersion7();
        var cloudEventJson = $$"""
            {
                "specversion": "1.0",
                "type": "BookWorm.Contracts.ProcessVectorBatchIntegrationEvent",
                "source": "urn:bookworm:book-worm-scheduler",
                "id": "{{eventId}}",
                "data": {
                    "id": "{{eventId}}",
                    "creationDate": "2026-11-20T00:00:00Z"
                }
            }
            """;

        var processor = new Mock<IVectorBatchProcessor>();
        processor.Setup(x => x.ProcessAsync(CancellationToken.None)).ReturnsAsync(3);
        var function = new ProcessVectorBatchFunction(
            processor.Object,
            Mock.Of<ILogger<ProcessVectorBatchFunction>>()
        );

        await function.ProcessVectorBatch(cloudEventJson, CancellationToken.None);

        processor.Verify(x => x.ProcessAsync(CancellationToken.None), Times.Once);
        processor.VerifyNoOtherCalls();
    }
}
