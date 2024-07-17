using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Catalog.Features.Books.Get;

public sealed class GetBookEndpoint : IEndpoint<Ok<BookDto>, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/books/{id:guid}",
                async (Guid id, ISender sender) => await HandleAsync(id, sender))
            .Produces<Ok<BookDto>>()
            .Produces<NotFound<ProblemDetails>>()
            .WithTags(nameof(Book))
            .WithName("Get Book")
            .MapToApiVersion(new(1, 0));

    public async Task<Ok<BookDto>> HandleAsync(Guid id, ISender sender, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetBookQuery(id), cancellationToken);

        var response = result.Value.ToBookDto();

        return TypedResults.Ok(response);
    }
}
