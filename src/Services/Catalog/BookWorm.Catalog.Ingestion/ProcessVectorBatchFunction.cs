using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Identity.Web;

namespace BookWorm.Catalog.Ingestion;

public sealed class ProcessVectorBatchFunction(IVectorBatchProcessor processor)
{
    [Function(nameof(ProcessVectorBatch))]
    public async Task<Results<UnauthorizedHttpResult, NoContent>> ProcessVectorBatch(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "vectors/batch")]
            HttpRequest request,
        CancellationToken cancellationToken
    )
    {
        var (isAuthenticated, _) = await request.HttpContext.AuthenticateAzureFunctionAsync();

        if (!isAuthenticated)
        {
            return TypedResults.Unauthorized();
        }

        await processor.ProcessAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
