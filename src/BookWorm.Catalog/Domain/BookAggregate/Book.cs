using System.Text.Json.Serialization;
using Ardalis.GuardClauses;
using BookWorm.Core.SeedWork;
using Pgvector;

namespace BookWorm.Catalog.Domain.BookAggregate;

public sealed class Book : EntityBase, IAggregateRoot, ISoftDelete
{
    private Book()
    {
        // EF Core
    }

    public Book(
        string name,
        string description,
        string imageUrl,
        decimal price,
        decimal priceSale,
        Status status,
        Guid categoryId,
        Guid publisherId,
        List<Guid> authorIds)
    {
        Name = Guard.Against.NullOrEmpty(name);
        Description = Guard.Against.NullOrEmpty(description);
        ImageUrl = imageUrl;
        Price = new(price, priceSale);
        Status = status;
        CategoryId = categoryId;
        PublisherId = publisherId;
        _bookAuthors = [..authorIds.Select(authorId => new BookAuthor(authorId))];
    }

    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public Price? Price { get; private set; }
    public Status Status { get; private set; }
    public bool IsDeleted { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public Guid? PublisherId { get; private set; }
    public Publisher? Publisher { get; private set; }
    [JsonIgnore] public Vector? Embedding { get; private set; }

    private readonly List<BookAuthor> _bookAuthors = [];

    public IReadOnlyCollection<BookAuthor> BookAuthors => _bookAuthors.AsReadOnly();

    public void Embed(Vector embedding) => Embedding = embedding;

    public void SetRating(double rating, int totalReviews)
    {
        AverageRating = Guard.Against.NegativeOrZero(totalReviews);
        TotalReviews = Guard.Against.NegativeOrZero(totalReviews);
    }
}
