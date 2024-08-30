using BookWorm.Catalog.Domain.BookAggregate;

namespace BookWorm.Catalog.Features.Books.Update;

public sealed record UpdateBookCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    decimal PriceSale,
    Status Status,
    Guid CategoryId,
    Guid PublisherId,
    List<Guid> AuthorIds) : ICommand<Result>;

public sealed class UpdateBookHandler(IRepository<Book> repository, IAiService aiService)
    : ICommandHandler<UpdateBookCommand, Result>
{
    public async Task<Result> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        var book = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, book);

        book.Update(
            request.Name,
            request.Description,
            request.Price,
            request.PriceSale,
            request.Status,
            request.CategoryId,
            request.PublisherId,
            request.AuthorIds);

        if (string.Compare(book.Name, request.Name, StringComparison.OrdinalIgnoreCase) != 0 ||
            string.Compare(book.Description, request.Description, StringComparison.OrdinalIgnoreCase) != 0)
        {
            var embedding = await aiService.GetEmbeddingAsync($"{book.Name} {book.Description}", cancellationToken);
            book.Embed(embedding);
        }

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
