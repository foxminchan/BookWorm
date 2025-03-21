using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
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
        Description = !string.IsNullOrWhiteSpace(description)
            ? description
            : throw new CatalogDomainException("Book description is required.");
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
    public Price? Price { get; private set; }
    public Status Status { get; private set; }
    public double AverageRating { get; private set; }
    public int TotalReviews { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; } = default!;
    public Guid? PublisherId { get; private set; }
    public Publisher? Publisher { get; private set; } = default!;

    [NotMapped]
    [JsonConverter(typeof(EmbeddingJsonConverter))]
    public ReadOnlyMemory<float> Embedding { get; init; }

    public IReadOnlyCollection<BookAuthor> BookAuthors => _bookAuthors.AsReadOnly();
    public bool IsDeleted { get; set; }

    public void Delete()
    {
        IsDeleted = true;
    }

    /// <summary>
    ///     Sets the metadata for the book.
    /// </summary>
    /// <param name="description">The description of the book.</param>
    /// <param name="categoryId">The category ID of the book.</param>
    /// <param name="publisherId">The publisher ID of the book.</param>
    /// <param name="authorIds">The list of author IDs associated with the book.</param>
    public void SetMetadata(
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
    public void Update(
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
        Description = !string.IsNullOrWhiteSpace(description)
            ? description
            : throw new CatalogDomainException("Book description is required.");
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
    }

    /// <summary>
    ///     Adds a rating to the book and updates the average rating.
    /// </summary>
    /// <param name="rating">The rating to add.</param>
    public void AddRating(int rating)
    {
        AverageRating = ((AverageRating * TotalReviews) + rating) / (TotalReviews + 1);
        TotalReviews++;
    }

    /// <summary>
    ///     Removes a rating from the book and updates the average rating.
    /// </summary>
    /// <param name="rating">The rating to remove.</param>
    public void RemoveRating(int rating)
    {
        AverageRating = ((AverageRating * TotalReviews) - rating) / (TotalReviews - 1);
        TotalReviews--;
    }

    /// <summary>
    ///     JSON converter for ReadOnlyMemory to handle embedding serialization and deserialization.
    /// </summary>
    internal class EmbeddingJsonConverter : JsonConverter<ReadOnlyMemory<float>>
    {
        /// <summary>
        ///     Reads and converts the JSON to ReadOnlyMemory.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>The converted ReadOnlyMemory.</returns>
        public override ReadOnlyMemory<float> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new InvalidOperationException(
                    $"JSON deserialization failed because the value type was {reader.TokenType} but should be {JsonTokenType.String}"
                );
            }

            var bytes = reader.GetBytesFromBase64();
            var floats = MemoryMarshal.Cast<byte, float>(bytes);
            return floats.ToArray();
        }

        /// <summary>
        ///     Writes the ReadOnlyMemory to JSON.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The serializer options.</param>
        public override void Write(
            Utf8JsonWriter writer,
            ReadOnlyMemory<float> value,
            JsonSerializerOptions options
        )
        {
            var bytes = MemoryMarshal.AsBytes(value.Span);
            writer.WriteBase64StringValue(bytes);
        }
    }
}
