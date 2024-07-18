using Ardalis.Specification;

namespace BookWorm.Catalog.Domain.BookAggregate.Specifications;

public static class BookSpecExpression
{
    public static ISpecificationBuilder<Book> ApplyOrdering(this ISpecificationBuilder<Book> builder,
        string? orderBy, bool isDescending)
    {
        return orderBy switch
        {
            nameof(Book.Name) => isDescending
                ? builder.OrderByDescending(x => x.Name)
                : builder.OrderBy(x => x.Name),
            nameof(Book.Price.OriginalPrice) => isDescending
                ? builder.OrderByDescending(x => x.Price!.OriginalPrice)
                : builder.OrderBy(x => x.Price!.OriginalPrice),
            nameof(Book.Price.DiscountPrice) => isDescending
                ? builder.OrderByDescending(x => x.Price!.DiscountPrice)
                : builder.OrderBy(x => x.Price!.DiscountPrice),
            nameof(Book.Status) => isDescending
                ? builder.OrderByDescending(x => x.Status)
                : builder.OrderBy(x => x.Status),
            _ => isDescending
                ? builder.OrderByDescending(x => x.Name)
                : builder.OrderBy(x => x.Name)
        };
    }

    public static ISpecificationBuilder<Book> ApplyPaging(this ISpecificationBuilder<Book> builder,
        int pageIndex, int pageSize)
    {
        return builder
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize);
    }
}
