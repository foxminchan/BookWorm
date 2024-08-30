using BookWorm.Catalog.Domain.BookAggregate;

namespace BookWorm.Catalog.Features.Books.Delete;

public sealed record DeleteBookCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteBookHandler(IRepository<Book> repository) : ICommandHandler<DeleteBookCommand, Result>
{
    public async Task<Result> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
    {
        var book = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, book);

        book.Delete();

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
