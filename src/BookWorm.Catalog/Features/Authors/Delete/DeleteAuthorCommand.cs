using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Catalog.Domain;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Catalog.Features.Authors.Delete;

public sealed record DeleteAuthorCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteAuthorHandler(IRepository<Author> repository) : ICommandHandler<DeleteAuthorCommand, Result>
{
    public async Task<Result> Handle(DeleteAuthorCommand request, CancellationToken cancellationToken)
    {
        var author = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, author);

        await repository.DeleteAsync(author, cancellationToken);

        return Result.Success();
    }
}
