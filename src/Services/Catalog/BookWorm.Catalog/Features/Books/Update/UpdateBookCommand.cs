using BookWorm.Chassis.CQRS.Command;

namespace BookWorm.Catalog.Features.Books.Update;

public sealed record UpdateBookCommand(
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
    [JsonIgnore]
    public string? ImageUrn { get; set; }
}

public sealed class UpdateBookHandler(IBookRepository repository)
    : ICommandHandler<UpdateBookCommand>
{
    public async Task<Unit> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
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
