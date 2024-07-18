using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.Features.Books.Update;

public sealed record UpdateBookRequest(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    decimal PriceSale,
    Status Status,
    Guid CategoryId,
    Guid PublisherId,
    List<Guid> AuthorIds);

public sealed class UpdateBookEndpoint : IEndpoint<Ok, UpdateBookRequest, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPut("/books",
                async (UpdateBookRequest request, ISender sender) => await HandleAsync(request, sender))
            .Produces<Ok>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(nameof(Book))
            .WithName("Update Book")
            .MapToApiVersion(new(1, 0));

    public async Task<Ok> HandleAsync(UpdateBookRequest request, ISender sender,
        CancellationToken cancellationToken = default)
    {
        await sender.Send(new UpdateBookCommand(
            request.Id,
            request.Name,
            request.Description,
            request.Price,
            request.PriceSale,
            request.Status,
            request.CategoryId,
            request.PublisherId,
            request.AuthorIds), cancellationToken);

        return TypedResults.Ok();
    }
}
