using BookWorm.Catalog.Domain.Events;

namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;

public sealed class Book() : AuditableEntity, IAggregateRoot, ISoftDelete
{
    private readonly List<BookAuthor> _bookAuthors = [];

    public Book(
        string name,
        string? description,
        string? image,
        decimal price,
        decimal? priceSale,
        Guid categoryId,
        Guid publisherId,
        Guid[] authorIds
    )
        : this()
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new CatalogDomainException("Book name is required.");
        Description = description;
        Image = image;
        Price = new(price, priceSale);
        Status = Status.InStock;
        CategoryId = categoryId;
        PublisherId = publisherId;
        _bookAuthors = [.. authorIds.Select(authorId => new BookAuthor(authorId))];
        RegisterDomainEvent(new BookCreatedEvent(this));
    }

    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public string? Image { get; private set; }

    [DisallowNull]
    public Price? Price { get; private set; }

    public Status Status { get; private set; }
    public double AverageRating { get; private set; }
    public int TotalReviews { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; } = null!;
    public Guid? PublisherId { get; private set; }
    public Publisher? Publisher { get; private set; } = null!;

    public IReadOnlyCollection<BookAuthor> BookAuthors => _bookAuthors.AsReadOnly();
    public bool IsDeleted { get; set; }

    public void Delete()
    {
        IsDeleted = true;
        RegisterDomainEvent(new BookChangedEvent($"{nameof(Book).ToLowerInvariant()}:{Id}"));
    }

    /// <summary>
    ///     Sets the metadata for the book.
    /// </summary>
    /// <param name="description">The description of the book.</param>
    /// <param name="categoryId">The category ID of the book.</param>
    /// <param name="publisherId">The publisher ID of the book.</param>
    /// <param name="authorIds">The list of author IDs associated with the book.</param>
    public Book SetMetadata(
        string? description,
        Guid categoryId,
        Guid publisherId,
        Guid[] authorIds
    )
    {
        Description = !string.IsNullOrWhiteSpace(description)
            ? description
            : throw new CatalogDomainException("Book description is required.");
        CategoryId = categoryId;
        PublisherId = publisherId;
        _bookAuthors.AddRange(authorIds.Select(authorId => new BookAuthor(authorId)));
        RegisterDomainEvent(new BookCreatedEvent(this));
        return this;
    }

    /// <summary>
    ///     Updates the book details.
    /// </summary>
    /// <param name="name">The name of the book.</param>
    /// <param name="description">The description of the book.</param>
    /// <param name="price">The price of the book.</param>
    /// <param name="priceSale">The sale price of the book.</param>
    /// <param name="image">The image of the book.</param>
    /// <param name="categoryId">The category ID of the book.</param>
    /// <param name="publisherId">The publisher ID of the book.</param>
    /// <param name="authorIds">The list of author IDs associated with the book.</param>
    public Book Update(
        string name,
        string? description,
        decimal price,
        decimal? priceSale,
        string? image,
        Guid categoryId,
        Guid publisherId,
        Guid[] authorIds
    )
    {
        var isChanged =
            string.Compare(Name, name, StringComparison.OrdinalIgnoreCase) != 0
            || string.Compare(Description, description, StringComparison.OrdinalIgnoreCase) != 0;

        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new CatalogDomainException("Book name is required.");
        Description = description;
        Price = new(price, priceSale);
        CategoryId = categoryId;
        PublisherId = publisherId;
        Image = image;
        _bookAuthors.Clear();
        _bookAuthors.AddRange(authorIds.Select(authorId => new BookAuthor(authorId)));

        if (isChanged)
        {
            RegisterDomainEvent(new BookUpdatedEvent(this));
        }

        RegisterDomainEvent(new BookChangedEvent($"{nameof(Book).ToLowerInvariant()}:{Id}"));
        return this;
    }

    /// <summary>
    ///     Adds a rating to the book and updates the average rating.
    /// </summary>
    /// <param name="rating">The rating to add.</param>
    public Book AddRating(int rating)
    {
        AverageRating = ((AverageRating * TotalReviews) + rating) / (TotalReviews + 1);
        TotalReviews++;
        RegisterDomainEvent(new BookChangedEvent($"{nameof(Book).ToLowerInvariant()}:{Id}"));
        return this;
    }

    /// <summary>
    ///     Removes a rating from the book and updates the average rating.
    /// </summary>
    /// <param name="rating">The rating to remove.</param>
    public Book RemoveRating(int rating)
    {
        if (TotalReviews <= 1)
        {
            AverageRating = 0;
            TotalReviews = 0;
        }
        else
        {
            AverageRating = ((AverageRating * TotalReviews) - rating) / (TotalReviews - 1);
            TotalReviews--;
        }

        RegisterDomainEvent(new BookChangedEvent($"{nameof(Book).ToLowerInvariant()}:{Id}"));
        return this;
    }
}
