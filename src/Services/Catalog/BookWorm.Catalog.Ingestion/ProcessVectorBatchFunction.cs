using System.Security.Claims;
using BookWorm.Constants.Aspire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Functions.Worker;

namespace BookWorm.Catalog.Ingestion;

public sealed class ProcessVectorBatchFunction(IVectorBatchProcessor processor)
{
    [Function(nameof(ProcessVectorBatch))]
    public async Task<
        Results<UnauthorizedHttpResult, ForbidHttpResult, NoContent>
    > ProcessVectorBatch(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "vectors/batch")]
            HttpRequest request,
        CancellationToken cancellationToken
    )
    {
        var authentication = await request.HttpContext.AuthenticateAsync(
            JwtBearerDefaults.AuthenticationScheme
        );

        if (!authentication.Succeeded || authentication.Principal is null)
        {
            return TypedResults.Unauthorized();
        }

        if (
            !string.Equals(
                authentication.Principal.FindFirstValue("azp"),
                Services.Scheduler,
                StringComparison.Ordinal
            )
        )
        {
            return TypedResults.Forbid();
        }

        await processor.ProcessAsync(cancellationToken);
        return TypedResults.NoContent();
    }
}
