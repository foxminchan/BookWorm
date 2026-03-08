using BookWorm.Chassis.Utilities.Guards;
using Mediator;

namespace BookWorm.Catalog.Features.Books.Update;

public sealed class UpdateBookCommand : ICommand
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IFormFile? Image { get; init; }
    public decimal Price { get; init; }
    public decimal? PriceSale { get; init; }
    public Guid CategoryId { get; init; }
    public Guid PublisherId { get; init; }
    public Guid[] AuthorIds { get; init; } = [];
    public bool IsRemoveImage { get; init; }

    [JsonIgnore]
    public string? ImageUrn { get; set; }
}

internal sealed class UpdateBookHandler(IBookRepository repository)
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
