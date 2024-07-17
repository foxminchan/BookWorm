using BookWorm.Catalog.Domain;
using BookWorm.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Catalog.Features.Authors.Create;

public sealed record CreateAuthorRequest(string Name);

public class CreateAuthorEndpoint : IEndpoint<Created<Guid>, CreateAuthorRequest, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/authors",
                async (CreateAuthorRequest request, ISender sender) => await HandleAsync(request, sender))
            .Produces<Created<Guid>>()
            .Produces<BadRequest<ProblemDetails>>()
            .WithTags(nameof(Author))
            .WithName("Create Author")
            .MapToApiVersion(new(1, 0));

    public async Task<Created<Guid>> HandleAsync(CreateAuthorRequest request, ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new CreateAuthorCommand(request.Name), cancellationToken);

        return TypedResults.Created($"/api/v1/authors/{result.Value}", result.Value);
    }
}
