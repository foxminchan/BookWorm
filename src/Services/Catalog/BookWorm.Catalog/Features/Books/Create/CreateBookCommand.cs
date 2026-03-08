using Mediator;

namespace BookWorm.Catalog.Features.Books.Create;

public sealed class CreateBookCommand : ICommand<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IFormFile? Image { get; init; }
    public decimal Price { get; init; }
    public decimal? PriceSale { get; init; }
    public Guid CategoryId { get; init; }
    public Guid PublisherId { get; init; }
    public Guid[] AuthorIds { get; init; } = [];

    [JsonIgnore]
    public string? ImageName { get; set; }
}

internal sealed class CreateBookHandler(IBookRepository repository)
    : ICommandHandler<CreateBookCommand, Guid>
{
    public async ValueTask<Guid> Handle(
        CreateBookCommand request,
        CancellationToken cancellationToken
    )
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
