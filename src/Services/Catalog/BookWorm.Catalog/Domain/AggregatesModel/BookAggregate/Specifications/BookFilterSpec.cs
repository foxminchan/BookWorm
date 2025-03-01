using BookWorm.SharedKernel.Specification.Builders;

namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;

public sealed class BookFilterSpec : Specification<Book>
{
    public BookFilterSpec(
        decimal? minPrice,
        decimal? maxPrice,
        Guid[]? categoryId,
        Guid[]? publisherId,
        Guid[]? authorIds,
        Guid[] ids
    )
    {
        Query.AsNoTracking();

        if (ids.Length > 0)
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

        if (categoryId is not null && categoryId.Length > 0)
        {
            Query.Where(book => categoryId.Contains(book.Category!.Id));
        }

        if (publisherId is not null && publisherId.Length > 0)
        {
            Query.Where(book => publisherId.Contains(book.Publisher!.Id));
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
        Guid[]? categoryId,
        Guid[]? publisherId,
        Guid[]? authorIds,
        Guid[] ids
    )
        : this(minPrice, maxPrice, categoryId, publisherId, authorIds, ids)
    {
        Query
            .Include(x => x.BookAuthors)
            .ThenInclude(x => x.Author)
            .ApplyOrdering(orderBy, isDescending)
            .ApplyPaging(pageIndex, pageSize);
    }

    public BookFilterSpec(Guid[] ids)
    {
        Query.AsNoTracking().Where(book => ids.Contains(book.Id));
    }
}
