using BookWorm.Constants.Core;

namespace BookWorm.Catalog.Features.Authors.Update;

public sealed class UpdateAuthorEndpoint : IEndpoint<NoContent, UpdateAuthorCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/authors",
                async (UpdateAuthorCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .ProducesPut()
            .WithTags(nameof(Author))
            .WithName(nameof(UpdateAuthorEndpoint))
            .WithSummary("Update Author")
            .WithDescription("Update an author if it exists")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin, Authorization.Policies.Vendor)
            .RequirePerUserRateLimit();
    }

    public async Task<NoContent> HandleAsync(
        UpdateAuthorCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }
}
