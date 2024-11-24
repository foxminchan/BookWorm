using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Catalog.Domain.BookAggregate.Specifications;

namespace BookWorm.Catalog.Features.Books.Get;

public sealed record GetBookQuery(Guid Id) : IQuery<Result<Book>>;

public sealed class GetBookHandler(IReadRepository<Book> repository)
    : IQueryHandler<GetBookQuery, Result<Book>>
{
    public async Task<Result<Book>> Handle(
        GetBookQuery request,
        CancellationToken cancellationToken
    )
    {
        var book = await repository.FirstOrDefaultAsync(
            new BookFilterSpec(request.Id),
            cancellationToken
        );

        if (book is null)
        {
            return Result.NotFound();
        }

        return book;
    }
}
