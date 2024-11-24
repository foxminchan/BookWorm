using BookWorm.Catalog.Domain.BookAggregate;

namespace BookWorm.Catalog.Features.Books.Update;

public sealed class UpdateBookEndpoint : IEndpoint<Ok, UpdateBookCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/books",
                async (UpdateBookCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Produces<Ok>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi()
            .WithTags(nameof(Book))
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Ok> HandleAsync(
        UpdateBookCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.Ok();
    }
}
