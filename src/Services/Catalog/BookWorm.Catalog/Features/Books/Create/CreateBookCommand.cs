using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Catalog.Features.Books.Create;

public sealed record CreateBookCommand(
    [FromForm] string Name,
    [FromForm] string Description,
    IFormFile? Image,
    [FromForm] decimal Price,
    [FromForm] decimal? PriceSale,
    [FromForm] Guid CategoryId,
    [FromForm] Guid PublisherId,
    [FromForm] Guid[] AuthorIds
) : ICommand<Guid>
{
    [JsonIgnore]
    public string? ImageName { get; set; }
}

public sealed class CreateBookHandler(IBookRepository repository)
    : ICommandHandler<CreateBookCommand, Guid>
{
    public async Task<Guid> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var book = new Book(
            request.Name,
            request.Description,
            request.ImageName,
            request.Price,
            request.PriceSale,
            request.CategoryId,
            request.PublisherId,
            request.AuthorIds
        );

        var result = await repository.AddAsync(book, cancellationToken);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return result.Id;
    }
}
