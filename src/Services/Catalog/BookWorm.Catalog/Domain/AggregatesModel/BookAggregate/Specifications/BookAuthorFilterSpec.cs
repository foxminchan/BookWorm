using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;

internal sealed class BookAuthorFilterSpec : Specification<Author>
{
    public BookAuthorFilterSpec(Guid id)
    {
        Query.AsNoTracking().Where(x => x.BookAuthors.Any(y => y.AuthorId == id));
    }
}
