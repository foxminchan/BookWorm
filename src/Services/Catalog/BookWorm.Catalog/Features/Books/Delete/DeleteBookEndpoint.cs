using BookWorm.Constants.Core;
using Mediator;

namespace BookWorm.Catalog.Features.Books.Delete;

internal sealed class DeleteBookEndpoint : IEndpoint<NoContent, BookId, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/books/{id}",
                async (
                    [Description("The unique identifier of the book to be deleted")] BookId id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesDelete()
            .WithTags(nameof(Book))
            .WithName(nameof(DeleteBookEndpoint))
            .WithSummary("Delete Book")
            .WithDescription("Delete a book if it exists")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequirePerUserRateLimit();
    }

    public async Task<NoContent> HandleAsync(
        BookId id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeleteBookCommand((Guid)id), cancellationToken);

        return TypedResults.NoContent();
    }
}
