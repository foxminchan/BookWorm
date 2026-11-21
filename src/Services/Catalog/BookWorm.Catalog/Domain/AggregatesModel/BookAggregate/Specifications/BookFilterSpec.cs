using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;

internal sealed class BookFilterSpec : Specification<Book>
{
    public BookFilterSpec(Guid[]? categoryIds, Guid[]? publisherIds)
    {
        Query.AsNoTracking();

        if (categoryIds is { Length: > 0 })
        {
            var typedCategoryIds = Array.ConvertAll(categoryIds, CategoryId.From);
            Query.Where(book =>
                book.CategoryId != null && typedCategoryIds.Contains(book.CategoryId.Value)
            );
        }

        if (publisherIds is not { Length: > 0 })
        {
            return;
        }

        var typedPublisherIds = Array.ConvertAll(publisherIds, PublisherId.From);
        Query.Where(book =>
            book.PublisherId != null && typedPublisherIds.Contains(book.PublisherId.Value)
        );
    }

    public BookFilterSpec(
        decimal? minPrice,
        decimal? maxPrice,
        Guid[]? categoryIds,
        Guid[]? publisherIds,
        Guid[]? authorIds,
        Guid[]? ids
    )
        : this(categoryIds, publisherIds)
    {
        if (ids is { Length: > 0 })
        {
            var typedIds = Array.ConvertAll(ids, BookId.From);
            Query.Where(book => typedIds.Contains(book.Id));
        }

        if (minPrice is not null)
        {
            Query.Where(book => book.Price!.OriginalPrice >= minPrice);
        }

        if (maxPrice is not null)
        {
            Query.Where(book => book.Price!.OriginalPrice <= maxPrice);
        }

        if (authorIds is not { Length: > 0 })
        {
            return;
        }

        var typedAuthorIds = Array.ConvertAll(authorIds, AuthorId.From);
        Query.Where(book =>
            book.BookAuthors.Any(author => typedAuthorIds.Contains(author.AuthorId))
        );
    }

    public BookFilterSpec(
        int pageIndex,
        int pageSize,
        string? orderBy,
        bool isDescending,
        decimal? minPrice,
        decimal? maxPrice,
        Guid[]? categoryIds,
        Guid[]? publisherIds,
        Guid[]? authorIds,
        Guid[]? ids
    )
        : this(minPrice, maxPrice, categoryIds, publisherIds, authorIds, ids)
    {
        Query
            .Include(x => x.BookAuthors)
                .ThenInclude(x => x.Author)
            .AsSplitQuery()
            .ApplyOrdering(orderBy, isDescending)
            .ApplyPaging(pageIndex, pageSize);
    }

    public BookFilterSpec(Guid[]? ids)
        : this(null, null, null, null, null, ids) { }
}
