using Ardalis.Result;
using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Catalog.Features.Books.Create;

public sealed record CreateBookCommand(
    string Name,
    string Description,
    string ImageUrl,
    decimal Price,
    decimal PriceSale,
    Status Status,
    Guid CategoryId,
    Guid PublisherId,
    List<Guid> AuthorIds) : ICommand<Result<Guid>>;

public sealed class CreateProductCommandHandler(IRepository<Book> repository)
    : ICommandHandler<CreateBookCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var book = new Book(
            request.Name,
            request.Description,
            request.ImageUrl,
            request.Price,
            request.PriceSale,
            request.Status,
            request.CategoryId,
            request.PublisherId,
            request.AuthorIds);

        var result = await repository.AddAsync(book, cancellationToken);

        return result.Id;
    }
}
