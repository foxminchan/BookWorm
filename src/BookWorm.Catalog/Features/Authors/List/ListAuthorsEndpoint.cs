﻿using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.Features.Authors.List;

public sealed class ListAuthorsEndpoint : IEndpoint<Ok<List<AuthorDto>>, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/authors", async (ISender sender) => await HandleAsync(sender))
            .Produces<Ok<List<AuthorDto>>>()
            .WithOpenApi()
            .WithTags(nameof(Author))
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Ok<List<AuthorDto>>> HandleAsync(
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(new ListAuthorsQuery(), cancellationToken);

        var response = result.Value.ToAuthorDtos();

        return TypedResults.Ok(response);
    }
}
