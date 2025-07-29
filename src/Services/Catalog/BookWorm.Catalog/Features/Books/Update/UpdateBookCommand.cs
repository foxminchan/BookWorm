using BookWorm.Chassis.CQRS.Command;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Catalog.Features.Books.Update;

public sealed record UpdateBookCommand(
    [FromForm] Guid Id,
    [FromForm] string Name,
    [FromForm] string Description,
    IFormFile? Image,
    [FromForm] decimal Price,
    [FromForm] decimal? PriceSale,
    [FromForm] Guid CategoryId,
    [FromForm] Guid PublisherId,
    [FromForm] Guid[] AuthorIds,
    [FromForm] bool IsRemoveImage = false
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
