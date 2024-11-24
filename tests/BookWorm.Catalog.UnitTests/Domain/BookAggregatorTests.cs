using BookWorm.Catalog.Domain.BookAggregate;

namespace BookWorm.Catalog.UnitTests.Domain;

public sealed class BookAggregatorTests
{
    [Fact]
    public void GivenValidParameters_ShouldInitializeCorrectly_WhenCreatingBook()
    {
        // Arrange
        const string name = "Test Book";
        const string description = "Test Description";
        const string imageUrl = "http://example.com/image.jpg";
        const decimal price = 100m;
        const decimal priceSale = 80m;
        const Status status = Status.InStock;
        var categoryId = Guid.NewGuid();
        var publisherId = Guid.NewGuid();
        Guid[] authorIds = [Guid.NewGuid(), Guid.NewGuid()];

        // Act
        var book = new Book(
            name,
            description,
            imageUrl,
            price,
            priceSale,
            status,
            categoryId,
            publisherId,
            authorIds
        );

        // Assert
        book.Name.Should().Be(name);
        book.Description.Should().Be(description);
        book.ImageUrl.Should().Be(imageUrl);
        book.Price.Should().Be(new Price(price, priceSale));
        book.Status.Should().Be(status);
        book.CategoryId.Should().Be(categoryId);
        book.PublisherId.Should().Be(publisherId);
        book.BookAuthors.Should().HaveCount(authorIds.Count());
        book.BookAuthors.Should().OnlyContain(ba => authorIds.Contains(ba.AuthorId));
    }

    [Fact]
    public void GivenValidUpdateParameters_ShouldUpdateCorrectly_WhenUpdatingBook()
    {
        // Arrange
        var book = new Book(
            "Original Name",
            "Original Description",
            "http://example.com/image.jpg",
            100m,
            80m,
            Status.InStock,
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );
        const string newName = "Updated Name";
        const string newDescription = "Updated Description";
        const decimal newPrice = 120m;
        const decimal newPriceSale = 90m;
        const Status newStatus = Status.OutOfStock;
        var newCategoryId = Guid.NewGuid();
        var newPublisherId = Guid.NewGuid();
        var newAuthorIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        book.Update(
            newName,
            newDescription,
            newPrice,
            newPriceSale,
            newStatus,
            newCategoryId,
            newPublisherId,
            newAuthorIds
        );

        // Assert
        book.Name.Should().Be(newName);
        book.Description.Should().Be(newDescription);
        book.Price.Should().Be(new Price(newPrice, newPriceSale));
        book.Status.Should().Be(newStatus);
        book.CategoryId.Should().Be(newCategoryId);
        book.PublisherId.Should().Be(newPublisherId);
        book.BookAuthors.Should().HaveCount(newAuthorIds.Count);
        book.BookAuthors.Should().OnlyContain(ba => newAuthorIds.Contains(ba.AuthorId));
    }

    [Fact]
    public void GivenEmbedding_ShouldSetEmbedding_WhenEmbeddingBook()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "http://example.com/image.jpg",
            100m,
            80m,
            Status.InStock,
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );
        var embedding = new Vector(new[] { 1.0f, 2.0f, 3.0f });

        // Act
        book.Embed(embedding);

        // Assert
        book.Embedding.Should().Be(embedding);
    }

    [Fact]
    public void GivenRating_ShouldUpdateAverageRatingAndTotalReviews_WhenAddingRating()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "http://example.com/image.jpg",
            100m,
            80m,
            Status.InStock,
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );
        const int rating = 4;

        // Act
        book.AddRating(rating);

        // Assert
        book.AverageRating.Should().Be(rating);
        book.TotalReviews.Should().Be(1);

        // Act again
        const int newRating = 5;
        book.AddRating(newRating);

        // Assert again
        book.AverageRating.Should().Be((rating + newRating) / 2.0);
        book.TotalReviews.Should().Be(2);
    }

    [Fact]
    public void GivenRating_ShouldUpdateAverageRatingAndTotalReviews_WhenRemovingRating()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "http://example.com/image.jpg",
            100m,
            80m,
            Status.InStock,
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );
        book.AddRating(4);
        book.AddRating(5);

        // Act
        book.RemoveRating(4);

        // Assert
        book.AverageRating.Should().Be(5);
        book.TotalReviews.Should().Be(1);
    }

    [Fact]
    public void GivenImageUrl_ShouldSetImageUrlToNull_WhenRemovingImage()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "http://example.com/image.jpg",
            100m,
            80m,
            Status.InStock,
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );

        // Act
        book.RemoveImage();

        // Assert
        book.ImageUrl.Should().BeNull();
    }

    [Fact]
    public void GivenDeletedBook_ShouldSetIsDeletedToTrue_WhenDeletingBook()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "http://example.com/image.jpg",
            100m,
            80m,
            Status.InStock,
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );

        // Act
        book.Delete();

        // Assert
        book.IsDeleted.Should().BeTrue();
    }
}
