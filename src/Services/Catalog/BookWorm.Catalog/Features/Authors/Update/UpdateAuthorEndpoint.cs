﻿namespace BookWorm.Catalog.Features.Authors.Update;

public sealed class UpdateAuthorEndpoint : IEndpoint<NoContent, UpdateAuthorCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/authors",
                async (UpdateAuthorCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .WithTags(nameof(Author))
            .WithName(nameof(UpdateAuthorEndpoint))
            .WithSummary("Update Author")
            .WithDescription("Update an author if it exists")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin);
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
