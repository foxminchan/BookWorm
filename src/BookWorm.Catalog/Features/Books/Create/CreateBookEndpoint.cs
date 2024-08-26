using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Catalog.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Catalog.Features.Books.Create;

public sealed record CreateBookRequest(
    string Name,
    string? Description,
    IFormFile? Image,
    decimal Price,
    decimal PriceSale,
    Status Status,
    Guid CategoryId,
    Guid PublisherId,
    Guid[] AuthorIds);

public sealed class CreateBookEndpoint : IEndpoint<Created<Guid>, CreateBookRequest, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/books",
                async ([FromForm] string name,
                        [FromForm] string? description,
                        [FromForm] decimal price,
                        [FromForm] decimal priceSale,
                        [FromForm] Status status,
                        [FromForm] Guid categoryId,
                        [FromForm] Guid publisherId,
                        [FromForm] Guid[] authorIds,
                        IFormFile? image,
                        ISender sender)
                    => await HandleAsync(
                        new(name, description, image, price, priceSale, status, categoryId, publisherId, authorIds),
                        sender))
            .AddEndpointFilter<FileValidationFilter>()
            .Produces<Created<Guid>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .DisableAntiforgery()
            .WithTags(nameof(Book))
            .WithName("Create Book")
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Created<Guid>> HandleAsync(CreateBookRequest request, ISender sender,
        CancellationToken cancellationToken = default)
    {
        CreateBookCommand command = new(
            request.Name,
            request.Description,
            request.Image,
            request.Price,
            request.PriceSale,
            request.Status,
            request.CategoryId,
            request.PublisherId,
            request.AuthorIds);

        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created($"/api/v1/books/{result.Value}", result.Value);
    }
}
