namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;

public sealed class BookAuthor() : Entity
{
    public BookAuthor(Guid authorId)
        : this()
    {
        AuthorId = authorId;
    }

    public Guid AuthorId { get; private set; }
    public Author Author { get; private set; } = default!;
    public Book Book { get; private set; } = default!;
}
