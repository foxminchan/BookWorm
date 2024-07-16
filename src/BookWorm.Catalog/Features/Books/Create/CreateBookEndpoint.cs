using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.Features.Books.Create;

public sealed record CreateBookRequest(
    string Name,
    string Description,
    string ImageUrl,
    decimal Price,
    decimal PriceSale,
    Status Status,
    Guid CategoryId,
    Guid PublisherId,
    List<Guid> AuthorIds);

public sealed class CreateBookEndpoint : IEndpoint<Created<Guid>, CreateBookRequest, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/books",
                async (CreateBookRequest request, ISender sender) => await HandleAsync(request, sender))
            .Produces<Created<Guid>>(StatusCodes.Status201Created)
            .DisableAntiforgery()
            .WithTags(nameof(Book))
            .WithName("Create Product")
            .MapToApiVersion(new(1, 0));

    public async Task<Created<Guid>> HandleAsync(CreateBookRequest request, ISender sender)
    {
        CreateBookCommand command = new(
            request.Name,
            request.Description,
            request.ImageUrl,
            request.Price,
            request.PriceSale,
            request.Status,
            request.CategoryId,
            request.PublisherId,
            request.AuthorIds);

        var result = await sender.Send(command);

        return TypedResults.Created($"/api/books/{result.Value}", result.Value);
    }
}
