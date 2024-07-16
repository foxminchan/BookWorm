using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Catalog.Domain.BookAggregate.Specifications;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Catalog.Features.Books.Get;

public sealed record GetBookQuery(Guid Id) : IQuery<Result<Book>>;

public sealed class GetBookHandler(IReadRepository<Book> repository) : IQueryHandler<GetBookQuery, Result<Book>>
{
    public async Task<Result<Book>> Handle(GetBookQuery request, CancellationToken cancellationToken)
    {
        var book = await repository.FirstOrDefaultAsync(new BookFilterSpec(request.Id), cancellationToken);

        Guard.Against.NotFound(request.Id, book);

        return book;
    }
}
