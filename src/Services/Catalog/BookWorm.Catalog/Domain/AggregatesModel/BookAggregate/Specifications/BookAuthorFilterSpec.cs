using BookWorm.SharedKernel.Specification.Builders;

namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;

public sealed class BookAuthorFilterSpec : Specification<Author>
{
    public BookAuthorFilterSpec(Guid id)
    {
        Query.Where(x => x.BookAuthors.Any(y => y.AuthorId == id));
    }
}
