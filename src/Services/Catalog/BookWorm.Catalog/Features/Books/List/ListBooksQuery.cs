using System.Globalization;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;
using BookWorm.Chassis.AI.Search;
using BookWorm.SharedKernel.Results;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.Features.Books.List;

public sealed record ListBooksQuery(
    [property: Description("Number of items to return in a single page of results")]
    [property: DefaultValue(Pagination.DefaultPageIndex)]
        int PageIndex = Pagination.DefaultPageIndex,
    [property: Description("Number of items to return in a single page of results")]
    [property: DefaultValue(Pagination.DefaultPageSize)]
        int PageSize = Pagination.DefaultPageSize,
    [property: Description("Property to order results by")]
    [property: DefaultValue(nameof(Book.Name))]
        string? OrderBy = nameof(Book.Name),
    [property: Description("Whether to order results in descending order")]
    [property: DefaultValue(false)]
        bool IsDescending = false,
    [property: Description("Search term to filter results by")]
    [property: DefaultValue(null)]
        string? Search = null,
    [property: Description("Minimum price to filter results by")]
    [property: DefaultValue(null)]
        decimal? MinPrice = null,
    [property: Description("Maximum price to filter results by")]
    [property: DefaultValue(null)]
        decimal? MaxPrice = null,
    [property: Description("Category IDs to filter results by")]
    [property: DefaultValue(null)]
        Guid[]? CategoryId = null,
    [property: Description("Publisher IDs to filter results by")]
    [property: DefaultValue(null)]
        Guid[]? PublisherId = null,
    [property: Description("Author IDs to filter results by")]
    [property: DefaultValue(null)]
        Guid[]? AuthorIds = null
) : IQuery<PagedResult<BookDto>>;

internal sealed class ListBooksHandler(
    ISearch search,
    IBookRepository repository,
    IFusionCache cache,
    IMapper<CachedBook, BookDto> mapper
) : IQueryHandler<ListBooksQuery, PagedResult<BookDto>>
{
    public async ValueTask<PagedResult<BookDto>> Handle(
        ListBooksQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid[] ids = [];
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var normalizedSearch = request.Search.Trim().ToLowerInvariant();

            ids = await cache.GetOrSetAsync(
                CatalogCacheKeys.BookSearch(normalizedSearch, request.PageSize),
                async ct =>
                {
                    var response = await search.SearchAsync(
                        request.Search,
                        [nameof(Book), request.Search],
                        request.PageSize,
                        ct
                    );

                    return response.Select(x => x.Id).ToArray();
                },
                new FusionCacheEntryOptions
                {
                    Duration = TimeSpan.FromMinutes(5),
                    DistributedCacheDuration = TimeSpan.FromMinutes(5),
                },
                [CatalogCacheKeys.BookSearchTag],
                cancellationToken
            );

            if (ids.Length == 0)
            {
                return [with([], request.PageIndex, request.PageSize, 0)];
            }
        }

        var canonicalKey = BuildListCacheKey(request, ids);

        var page = await cache.GetOrSetAsync(
            CatalogCacheKeys.BookList(canonicalKey),
            async ct =>
            {
                var filterSpec = new BookFilterSpec(
                    request.PageIndex,
                    request.PageSize,
                    request.OrderBy,
                    request.IsDescending,
                    request.MinPrice,
                    request.MaxPrice,
                    request.CategoryId,
                    request.PublisherId,
                    request.AuthorIds,
                    ids
                );

                var books = await repository.ListAsync(filterSpec, ct);

                long totalItems;
                if (ids.Length > 0 && ids.Length < request.PageSize && request.PageIndex == 1)
                {
                    totalItems = books.Count;
                }
                else
                {
                    var countSpec = new BookFilterSpec(
                        request.MinPrice,
                        request.MaxPrice,
                        request.CategoryId,
                        request.PublisherId,
                        request.AuthorIds,
                        ids
                    );

                    totalItems = await repository.CountAsync(countSpec, ct);
                }

                return new CachedBookPage([.. books.Select(CachedBook.FromBook)], totalItems);
            },
            new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromSeconds(60),
                DistributedCacheDuration = TimeSpan.FromSeconds(60),
            },
            [CatalogCacheKeys.BookListTag],
            cancellationToken
        );

        var results = mapper.Map(page.Items);

        return [with(results, request.PageIndex, request.PageSize, page.TotalItems)];
    }

    private static string BuildListCacheKey(ListBooksQuery request, Guid[] searchIds)
    {
        var orderBy = string.IsNullOrWhiteSpace(request.OrderBy)
            ? string.Empty
            : request.OrderBy.Trim().ToLowerInvariant();

        return string.Create(
            CultureInfo.InvariantCulture,
            $"p={request.PageIndex}|s={request.PageSize}|o={orderBy}|d={request.IsDescending}|min={request.MinPrice}|max={request.MaxPrice}|c={FormatGuids(request.CategoryId)}|pub={FormatGuids(request.PublisherId)}|a={FormatGuids(request.AuthorIds)}|ids={FormatGuids(searchIds)}"
        );
    }

    private static string FormatGuids(Guid[]? guids)
    {
        if (guids is null or { Length: 0 })
        {
            return string.Empty;
        }

        return string.Join(",", guids.OrderBy(id => id).Select(id => id.ToString("N")));
    }
}
