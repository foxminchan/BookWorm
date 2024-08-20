namespace BookWorm.Catalog.Domain.BookAggregate.Specifications;

public sealed class BookFilterSpec : Specification<Book>
{
    public BookFilterSpec(
        int pageIndex,
        int pageSize,
        string? orderBy,
        bool isDescending,
        Status[]? statuses,
        Guid[]? categoryId,
        Guid[]? publisherId,
        Guid[]? authorIds,
        Vector? vector)
    {
        Query.Where(x => !x.IsDeleted);

        if (statuses is not null)
        {
            Query.Where(book => statuses.Contains(book.Status));
        }

        if (categoryId is not null)
        {
            Query.Where(book => categoryId.Contains(book.Category!.Id));
        }

        if (publisherId is not null)
        {
            Query.Where(book => publisherId.Contains(book.Publisher!.Id));
        }

        if (authorIds is not null)
        {
            Query.Where(book => book.BookAuthors.Any(author => authorIds.Contains(author.Id)));
        }

        if (vector is not null)
        {
            Query.OrderBy(c => c.Embedding!.CosineDistance(vector));
        }
        else
        {
            Query.ApplyOrdering(orderBy, isDescending);
        }

        Query
            .Include(x => x.BookAuthors)
            .ThenInclude(x => x.Author)
            .ApplyPaging(pageIndex, pageSize);
    }

    public BookFilterSpec(Guid id)
    {
        Query.Where(x => x.Id == id && !x.IsDeleted);
    }
}
