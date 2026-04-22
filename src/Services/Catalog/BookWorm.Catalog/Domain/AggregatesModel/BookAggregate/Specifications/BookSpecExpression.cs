using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;

internal static class BookSpecExpression
{
    extension(ISpecificationBuilder<Book> builder)
    {
        public ISpecificationBuilder<Book> ApplyOrdering(string? orderBy, bool isDescending)
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
                    : builder.OrderBy(x => x.Name),
            };
        }

        public void ApplyPaging(int pageIndex, int pageSize)
        {
            builder.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
    }
}
