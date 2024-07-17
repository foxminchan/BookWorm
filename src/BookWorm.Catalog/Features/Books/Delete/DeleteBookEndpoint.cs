using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Catalog.Features.Books.Delete;

public sealed class DeleteBookEndpoint : IEndpoint<NoContent, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapDelete("/books/{id}",
                async (Guid id, ISender sender) => await HandleAsync(id, sender))
            .Produces<NoContent>(StatusCodes.Status204NoContent)
            .Produces<NotFound<ProblemDetails>>(StatusCodes.Status404NotFound)
            .Produces<BadRequest<ProblemDetails>>(StatusCodes.Status400BadRequest)
            .WithTags(nameof(Book))
            .WithName("Delete Book")
            .MapToApiVersion(new(1, 0));

    public async Task<NoContent> HandleAsync(Guid id, ISender sender, CancellationToken cancellationToken = default)
    {
        await sender.Send(new DeleteBookCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}
