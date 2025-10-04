using BookWorm.Chassis.CQRS.Command;

namespace BookWorm.Catalog.Features.Books.Create;

public sealed record CreateBookCommand(
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
