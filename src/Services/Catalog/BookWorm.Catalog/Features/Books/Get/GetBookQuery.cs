using BookWorm.Chassis.Utilities.Guards;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.Features.Books.Get;

public sealed record GetBookQuery(BookId Id) : IQuery<BookDto>;

internal sealed class GetBookHandler(
    IBookRepository repository,
    IFusionCache cache,
    IMapper<CachedBook, BookDto> mapper
) : IQueryHandler<GetBookQuery, BookDto>
{
    public async ValueTask<BookDto> Handle(
        GetBookQuery request,
        CancellationToken cancellationToken
    )
    {
        var book = await cache.GetOrSetAsync(
            CatalogCacheKeys.BookDetail(request.Id),
            async ctx =>
            {
                var book = await repository.GetByIdAsync((Guid)request.Id, ctx);

                Guard.Against.NotFound(book, (Guid)request.Id);

                return CachedBook.FromBook(book);
            },
            tags: [CatalogCacheKeys.BookTag],
            token: cancellationToken
        );

        return mapper.Map(book);
    }
}
