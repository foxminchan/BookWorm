namespace BookWorm.Catalog.Features.Books.Delete;

public sealed record DeleteBookCommand(Guid Id) : ICommand;

public sealed class DeleteBookHandler(IBookRepository repository)
    : ICommandHandler<DeleteBookCommand>
{
    public async Task<Unit> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
    {
        var book = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(book, request.Id);

        book.Delete();

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
