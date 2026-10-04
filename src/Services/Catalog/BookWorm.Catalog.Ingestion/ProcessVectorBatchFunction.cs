using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace BookWorm.Catalog.Ingestion;

public sealed class ProcessVectorBatchFunction(IVectorBatchProcessor processor)
{
    [Function(nameof(ProcessVectorBatch))]
    public async Task<HttpResponseData> ProcessVectorBatch(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "vectors/batch")]
            HttpRequestData request,
        CancellationToken cancellationToken
    )
    {
        await processor.ProcessAsync(cancellationToken);
        return request.CreateResponse(HttpStatusCode.NoContent);
    }
}
