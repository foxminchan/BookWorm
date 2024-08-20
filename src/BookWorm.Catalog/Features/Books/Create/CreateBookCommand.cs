using BookWorm.Catalog.Domain.BookAggregate;

namespace BookWorm.Catalog.Features.Books.Create;

public sealed record CreateBookCommand(
    string Name,
    string? Description,
    IFormFile? Image,
    decimal Price,
    decimal PriceSale,
    Status Status,
    Guid CategoryId,
    Guid PublisherId,
    List<Guid> AuthorIds) : ICommand<Result<Guid>>;

public sealed class CreateBookHandler(
    IRepository<Book> repository,
    IAzuriteService azurite,
    IAiService aiService) : ICommandHandler<CreateBookCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var imageUrl = await UploadProductImagesAsync(request.Image, cancellationToken);

        var book = new Book(
            request.Name,
            request.Description,
            imageUrl,
            request.Price,
            request.PriceSale,
            request.Status,
            request.CategoryId,
            request.PublisherId,
            request.AuthorIds);

        var vector = await aiService.GetEmbeddingAsync($"{book.Name} {book.Description}", cancellationToken);

        book.Embed(vector);

        var result = await repository.AddAsync(book, cancellationToken);

        return result.Id;
    }

    private async Task<string?> UploadProductImagesAsync(IFormFile? imageFile, CancellationToken cancellationToken)
    {
        if (imageFile is null)
        {
            return null;
        }

        var imageUrl = await azurite.UploadFileAsync(imageFile, cancellationToken);

        return imageUrl;
    }
}
