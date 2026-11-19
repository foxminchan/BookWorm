using System.Text.Json;
using BookWorm.Catalog.Ingestion;
using BookWorm.Contracts;
using Microsoft.Extensions.Logging;

namespace BookWorm.Catalog.ContractTests.Consumers;

[Category("PactConsumer")]
public sealed class ProcessVectorBatchConsumerTests
{
    [Test]
    public async Task GivenProcessVectorBatchEvent_WhenPublished_ThenConsumerShouldConsumeItAndProcessVectorBatch()
    {
        // Arrange
        var processorMock = new Mock<IVectorBatchProcessor>();
        processorMock.Setup(x => x.ProcessAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var function = new ProcessVectorBatchFunction(
            processorMock.Object,
            Mock.Of<ILogger<ProcessVectorBatchFunction>>()
        );
        var @event = new ProcessVectorBatchIntegrationEvent();

        // Act
        await PactTests.Helper.VerifyConsumerMessageAsync(
            "Catalog",
            "Scheduler",
            @event,
            async pactEvent =>
                await function.ProcessVectorBatch(
                    JsonSerializer.Serialize(
                        pactEvent,
                        PactMessageSerializationContext.Default.ProcessVectorBatchIntegrationEvent
                    ),
                    CancellationToken.None
                )
        );

        // Assert
        processorMock.Verify(x => x.ProcessAsync(It.IsAny<CancellationToken>()), Times.Once);
        processorMock.VerifyNoOtherCalls();
    }
}
