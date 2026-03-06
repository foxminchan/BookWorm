using Mediator;

namespace BookWorm.Catalog.Features.Books.Create;

public sealed class CreateBookCommand(
    string Name,
    string Description,
    IFormFile? Image,
    decimal Price,
    decimal? PriceSale,
    Guid CategoryId,
    Guid PublisherId,
    Guid[] AuthorIds
) : ICommand<Guid>
{
    public string Name { get; } = Name;
    public string Description { get; } = Description;
    public IFormFile? Image { get; } = Image;
    public decimal Price { get; } = Price;
    public decimal? PriceSale { get; } = PriceSale;
    public Guid CategoryId { get; } = CategoryId;
    public Guid PublisherId { get; } = PublisherId;
    public Guid[] AuthorIds { get; } = AuthorIds;

    [JsonIgnore]
    public string? ImageName { get; set; }
}

public sealed class CreateBookHandler(IBookRepository repository)
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
