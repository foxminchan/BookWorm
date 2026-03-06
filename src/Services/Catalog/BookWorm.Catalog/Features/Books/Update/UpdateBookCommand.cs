using BookWorm.Chassis.Utilities.Guards;
using Mediator;

namespace BookWorm.Catalog.Features.Books.Update;

public sealed class UpdateBookCommand(
    Guid Id,
    string Name,
    string Description,
    IFormFile? Image,
    decimal Price,
    decimal? PriceSale,
    Guid CategoryId,
    Guid PublisherId,
    Guid[] AuthorIds,
    bool IsRemoveImage = false
) : ICommand
{
    public Guid Id { get; } = Id;
    public string Name { get; } = Name;
    public string Description { get; } = Description;
    public IFormFile? Image { get; } = Image;
    public decimal Price { get; } = Price;
    public decimal? PriceSale { get; } = PriceSale;
    public Guid CategoryId { get; } = CategoryId;
    public Guid PublisherId { get; } = PublisherId;
    public Guid[] AuthorIds { get; } = AuthorIds;
    public bool IsRemoveImage { get; } = IsRemoveImage;

    [JsonIgnore]
    public string? ImageUrn { get; set; }
}

public sealed class UpdateBookHandler(IBookRepository repository)
    : ICommandHandler<UpdateBookCommand>
{
    public async ValueTask<Unit> Handle(
        UpdateBookCommand request,
        CancellationToken cancellationToken
    )
    {
        var book = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(book, request.Id);

        var imageName = request.IsRemoveImage ? null : request.ImageUrn ?? book.Image;

        book.Update(
            request.Name,
            request.Description,
            request.Price,
            request.PriceSale,
            imageName,
            request.CategoryId,
            request.PublisherId,
            request.AuthorIds
        );

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
