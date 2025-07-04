using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;

public sealed class BookFilterSpec : Specification<Book>
{
    public BookFilterSpec(Guid[]? categoryIds, Guid[]? publisherIds)
    {
        Query.AsNoTracking();

        if (categoryIds is not null && categoryIds.Length > 0)
        {
            Query.Where(book => categoryIds.Contains(book.Category!.Id));
        }

        if (publisherIds is not null && publisherIds.Length > 0)
        {
            Query.Where(book => publisherIds.Contains(book.Publisher!.Id));
        }
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
        if (ids is not null && ids.Length > 0)
        {
            Query.Where(book => ids.Contains(book.Id));
        }

        if (minPrice is not null)
        {
            Query.Where(book => book.Price!.OriginalPrice >= minPrice);
        }

        if (maxPrice is not null)
        {
            Query.Where(book => book.Price!.OriginalPrice <= maxPrice);
        }

        if (authorIds is not null && authorIds.Length > 0)
        {
            Query.Where(book => book.BookAuthors.Any(author => authorIds.Contains(author.Id)));
        }
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
            .ApplyOrdering(orderBy, isDescending)
            .ApplyPaging(pageIndex, pageSize);
    }

    public BookFilterSpec(Guid[]? ids)
        : this(null, null, null, null, null, ids) { }
}
