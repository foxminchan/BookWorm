using Ardalis.Specification;

namespace BookWorm.Catalog.Domain.BookAggregate.Specifications;

public sealed class BookAuthorFilterSpec : Specification<Author>
{
    public BookAuthorFilterSpec(Guid id)
    {
        Query.Where(x => x.BookAuthors.Any(y => y.AuthorId == id));
    }
}
