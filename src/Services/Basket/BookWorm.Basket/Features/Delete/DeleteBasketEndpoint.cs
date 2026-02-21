using BookWorm.Constants.Core;
using Mediator;

namespace BookWorm.Basket.Features.Delete;

public sealed class DeleteBasketEndpoint : IEndpoint<NoContent, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(string.Empty, async (ISender sender) => await HandleAsync(sender))
            .ProducesDelete()
            .WithTags(nameof(Basket))
            .WithName(nameof(DeleteBasketEndpoint))
            .WithSummary("Delete Basket")
            .WithDescription("Delete a basket by its unique identifier")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<NoContent> HandleAsync(
        ISender request,
        CancellationToken cancellationToken = default
    )
    {
        await request.Send(new DeleteBasketCommand(), cancellationToken);

        return TypedResults.NoContent();
    }
}
