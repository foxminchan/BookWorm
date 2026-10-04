using System.Net;
using BookWorm.Catalog.Ingestion;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace BookWorm.Catalog.UnitTests.Infrastructure;

public sealed class ProcessVectorBatchFunctionTests
{
    [Test]
    public async Task GivenAnonymousRequest_WhenInvoked_ThenShouldProcessOneVectorBatch()
    {
        using var cancellation = new CancellationTokenSource();
        var processor = new Mock<IVectorBatchProcessor>();
        processor.Setup(x => x.ProcessAsync(cancellation.Token)).ReturnsAsync(7);
        var function = new ProcessVectorBatchFunction(processor.Object);

        var response = await function.ProcessVectorBatch(CreateRequest(), cancellation.Token);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        processor.Verify(x => x.ProcessAsync(cancellation.Token), Times.Once);
        processor.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenProcessingFails_WhenInvoked_ThenShouldSurfaceFailure()
    {
        var processor = new Mock<IVectorBatchProcessor>();
        processor
            .Setup(x => x.ProcessAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Embedding service unavailable"));
        var function = new ProcessVectorBatchFunction(processor.Object);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            function.ProcessVectorBatch(CreateRequest(), CancellationToken.None)
        );
    }

    [Test]
    public async Task GivenProcessingCancelled_WhenInvoked_ThenShouldPropagateCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var processor = new Mock<IVectorBatchProcessor>();
        processor
            .Setup(x => x.ProcessAsync(cancellation.Token))
            .Returns(Task.FromCanceled<int>(cancellation.Token));
        var function = new ProcessVectorBatchFunction(processor.Object);

        await Should.ThrowAsync<OperationCanceledException>(() =>
            function.ProcessVectorBatch(CreateRequest(), cancellation.Token)
        );
    }

    private static HttpRequestData CreateRequest()
    {
        var context = Mock.Of<FunctionContext>();
        var response = new Mock<HttpResponseData>(context);
        response.SetupProperty(x => x.StatusCode);
        var request = new Mock<HttpRequestData>(context);
        request.SetupGet(x => x.Headers).Returns(new HttpHeadersCollection());
        request.Setup(x => x.CreateResponse()).Returns(response.Object);
        return request.Object;
    }
}
