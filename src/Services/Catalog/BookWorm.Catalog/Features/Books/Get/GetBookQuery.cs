using Microsoft.Extensions.Caching.Hybrid;

namespace BookWorm.Catalog.Features.Books.Get;

public sealed record GetBookQuery(Guid Id) : IQuery<BookDto>;

public sealed class GetBookHandler(
    IBookRepository repository,
    HybridCache cache,
    IMapper<Book, BookDto> mapper
) : IQueryHandler<GetBookQuery, BookDto>
{
    public async Task<BookDto> Handle(GetBookQuery request, CancellationToken cancellationToken)
    {
        var tag = nameof(Book).ToLower();

        var book = await cache.GetOrCreateAsync(
            $"{tag}:{request.Id}",
            async ctx =>
            {
                var book = await repository.GetByIdAsync(request.Id, ctx);

                Guard.Against.NotFound(book, $"Book with id {request.Id} not found.");

                return book;
            },
            tags: [tag],
            cancellationToken: cancellationToken
        );

        return mapper.MapToDto(book);
    }
}
