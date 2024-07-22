using BookWorm.Catalog.Domain;
using BookWorm.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.Features.Categories.List;

public sealed class ListCategoriesEndpoint : IEndpoint<Ok<List<CategoryDto>>, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", async (ISender sender) => await HandleAsync(sender))
            .Produces<Ok<List<CategoryDto>>>()
            .CacheOutput(policy => policy.Expire(TimeSpan.FromHours(1)))
            .WithTags(nameof(Category))
            .WithName("List Categories")
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Ok<List<CategoryDto>>> HandleAsync(ISender sender, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new ListCategoriesQuery(), cancellationToken);

        return TypedResults.Ok(result.Value.ToCategoryDtos());
    }
}
