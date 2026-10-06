using System.Net;
using BookWorm.Scheduler.Clients;
using BookWorm.Scheduler.Jobs;
using Microsoft.Extensions.Logging;
using Quartz;
using Refit;

namespace BookWorm.Scheduler.UnitTests;

public sealed class ProcessVectorBatchJobTests
{
    [Test]
    public async Task GivenScheduledBatch_WhenExecuting_ThenShouldSendOneAnonymousPost()
    {
        using var handler = new RecordingHandler(HttpStatusCode.NoContent);
        using var client = new HttpClient(handler)
        {
            BaseAddress = new("http://catalog-ingestion"),
        };
        var api = RestService.For<ICatalogIngestionApi>(
            client,
            RefitSettings.ForJsonContext(CatalogIngestionSerializationContext.Default)
        );
        var job = new ProcessVectorBatchJob(api, Mock.Of<ILogger<ProcessVectorBatchJob>>());

        await job.Execute(Mock.Of<IJobExecutionContext>());

        handler.RequestCount.ShouldBe(1);
        handler.Method.ShouldBe(HttpMethod.Post);
        handler.Uri!.AbsolutePath.ShouldBe("/api/vectors/batch");
        handler.HeaderCount.ShouldBe(0);
    }

    [Test]
    public async Task GivenFailedBatch_WhenExecuting_ThenShouldSurfaceJobFailureWithoutRetrying()
    {
        using var handler = new RecordingHandler(HttpStatusCode.InternalServerError);
        using var client = new HttpClient(handler)
        {
            BaseAddress = new("http://catalog-ingestion"),
        };
        var api = RestService.For<ICatalogIngestionApi>(
            client,
            RefitSettings.ForJsonContext(CatalogIngestionSerializationContext.Default)
        );
        var job = new ProcessVectorBatchJob(api, Mock.Of<ILogger<ProcessVectorBatchJob>>());

        await Should.ThrowAsync<JobExecutionException>(() =>
            job.Execute(Mock.Of<IJobExecutionContext>()).AsTask()
        );
        handler.RequestCount.ShouldBe(1);
    }

    [Test]
    public async Task GivenCancellation_WhenExecuting_ThenShouldNotSendRequest()
    {
        var apiMock = new Mock<ICatalogIngestionApi>();
        var job = new ProcessVectorBatchJob(
            apiMock.Object,
            Mock.Of<ILogger<ProcessVectorBatchJob>>()
        );
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() =>
            job.Execute(Mock.Of<IJobExecutionContext>(), cancellation.Token).AsTask()
        );
        apiMock.VerifyNoOtherCalls();
    }

    private sealed class RecordingHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }
        public HttpMethod? Method { get; private set; }
        public Uri? Uri { get; private set; }
        public int HeaderCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            RequestCount++;
            Method = request.Method;
            Uri = request.RequestUri;
            HeaderCount = request.Headers.Count();
            return Task.FromResult(new HttpResponseMessage(statusCode));
        }
    }
}
