﻿namespace BookWorm.Catalog.Features.Categories.List;

public sealed class ListCategoriesEndpoint : IEndpoint<Ok<IReadOnlyList<CategoryDto>>, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", async (ISender sender) => await HandleAsync(sender))
            .Produces<IReadOnlyList<CategoryDto>>()
            .WithTags(nameof(Category))
            .WithName(nameof(ListCategoriesEndpoint))
            .WithSummary("List Categories")
            .WithDescription("List all categories")
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Ok<IReadOnlyList<CategoryDto>>> HandleAsync(
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(new ListCategoriesQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }
}
