namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;

public sealed class BookAuthor() : Entity
{
    public BookAuthor(AuthorId authorId)
        : this()
    {
        AuthorId = authorId;
    }

    public AuthorId AuthorId { get; private set; }
    public Author Author { get; private set; } = null!;
    public Book Book { get; private set; } = null!;
}
