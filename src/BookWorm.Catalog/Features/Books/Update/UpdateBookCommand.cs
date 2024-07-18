using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Core.SharedKernel;

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

public sealed class UpdateBookHandler(IRepository<Book> repository) : ICommandHandler<UpdateBookCommand, Result>
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

        await repository.UpdateAsync(book, cancellationToken);

        return Result.Success();
    }
}
