using BookWorm.Constants.Core;
using Mediator;

namespace BookWorm.Catalog.Features.Publishers.Create;

public sealed class CreatePublisherEndpoint : IEndpoint<Ok<Guid>, CreatePublisherCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/publishers",
                async (CreatePublisherCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .ProducesPostWithoutLocation<Guid>()
            .WithTags(nameof(Publisher))
            .WithName(nameof(CreatePublisherEndpoint))
            .WithSummary("Create Publisher")
            .WithDescription("Creates a new publisher")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<Guid>> HandleAsync(
        CreatePublisherCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}
