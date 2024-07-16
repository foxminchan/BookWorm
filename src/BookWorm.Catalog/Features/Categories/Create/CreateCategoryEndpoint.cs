using BookWorm.Catalog.Domain;
using BookWorm.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.Features.Categories.Create;

public sealed record CreateCategoryRequest(string Name);

public sealed class CreateCategoryEndpoint : IEndpoint<Created<Guid>, CreateCategoryRequest, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/categories",
                async (CreateCategoryRequest request, ISender sender) => await HandleAsync(request, sender))
            .Produces<Created<Guid>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithTags(nameof(Category))
            .WithName("Create Category")
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Created<Guid>> HandleAsync(CreateCategoryRequest request, ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new CreateCategoryCommand(request.Name), cancellationToken);

        return TypedResults.Created($"/api/v1/categories/{result.Value}", result.Value);
    }
}
