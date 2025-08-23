﻿using BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;
using BookWorm.Catalog.Domain.Events;
using BookWorm.Catalog.Domain.Exceptions;

namespace BookWorm.Catalog.UnitTests.Domain;

public sealed class BookAggregatorTests
{
    [Test]
    public void GivenValidParameters_WhenCreatingBook_ThenShouldInitializeCorrectly()
    {
        // Arrange
        const string name = "Clean Code";
        const string description = "A handbook of agile software craftsmanship";
        const string image = "example.jpg";
        const decimal originalPrice = 44.99m;
        const decimal salePrice = 39.99m;
        var categoryId = Guid.CreateVersion7();
        var publisherId = Guid.CreateVersion7();
        Guid[] authorIds = [Guid.CreateVersion7(), Guid.CreateVersion7()];

        // Act
        var book = new Book(
            name,
            description,
            image,
            originalPrice,
            salePrice,
            categoryId,
            publisherId,
            authorIds
        );

        // Assert
        book.ShouldNotBeNull();
        book.Name.ShouldBe(name);
        book.Description.ShouldBe(description);
        book.Image.ShouldBe(image);
        book.Price.ShouldBe(new(originalPrice, salePrice));
        book.Status.ShouldBe(Status.InStock);
        book.CategoryId.ShouldBe(categoryId);
        book.PublisherId.ShouldBe(publisherId);
        book.BookAuthors.ShouldNotBeNull();
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    public void GivenEmptyOrNullName_WhenCreatingBook_ThenShouldThrowException(string? name)
    {
        // Arrange
        const string description = "Test Description";
        const string image = "test.jpg";
        const decimal price = 19.99m;
        decimal? priceSale = 15.99m;
        var categoryId = Guid.CreateVersion7();
        var publisherId = Guid.CreateVersion7();
        Guid[] authorIds = [Guid.CreateVersion7()];

        // Act & Assert
        Should
            .Throw<CatalogDomainException>(() =>
                new Book(
                    name!,
                    description,
                    image,
                    price,
                    priceSale,
                    categoryId,
                    publisherId,
                    authorIds
                )
            )
            .Message.ShouldBe("Book name is required.");
    }

    [Test]
    public void GivenValidParameters_WhenUpdatingBook_ThenShouldUpdateCorrectly()
    {
        // Arrange
        var book = new Book(
            "Original Name",
            "Original Description",
            "original.jpg",
            19.99m,
            15.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        const string newName = "Updated Name";
        const string newDescription = "Updated Description";
        const decimal newPrice = 29.99m;
        decimal? newPriceSale = 24.99m;
        const string newImage = "updated.jpg";
        var newCategoryId = Guid.CreateVersion7();
        var newPublisherId = Guid.CreateVersion7();
        var newAuthorIds = new[] { Guid.CreateVersion7(), Guid.CreateVersion7() };

        // Act
        book.Update(
            newName,
            newDescription,
            newPrice,
            newPriceSale,
            newImage,
            newCategoryId,
            newPublisherId,
            newAuthorIds
        );

        // Assert
        book.Name.ShouldBe(newName);
        book.Description.ShouldBe(newDescription);
        book.Image.ShouldBe(newImage);
        book.Price!.OriginalPrice.ShouldBe(newPrice);
        book.Price.DiscountPrice.ShouldBe(newPriceSale);
        book.CategoryId.ShouldBe(newCategoryId);
        book.PublisherId.ShouldBe(newPublisherId);
        book.BookAuthors.Count.ShouldBe(newAuthorIds.Length);
        book.DomainEvents.ShouldContain(e => e is BookUpdatedEvent);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    public void GivenEmptyOrNullName_WhenUpdatingBook_ThenShouldThrowException(string? newName)
    {
        // Arrange
        var book = new Book(
            "Original Name",
            "Original Description",
            "original.jpg",
            19.99m,
            15.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        // Act & Assert
        Should
            .Throw<CatalogDomainException>(() =>
                book.Update(
                    newName!,
                    "Updated Description",
                    29.99m,
                    24.99m,
                    "updated.jpg",
                    Guid.CreateVersion7(),
                    Guid.CreateVersion7(),
                    [Guid.CreateVersion7()]
                )
            )
            .Message.ShouldBe("Book name is required.");
    }

    [Test]
    public void GivenNewRating_WhenAddingRating_ThenShouldUpdateAverageRating()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "test.jpg",
            19.99m,
            15.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        // Act
        book.AddRating(5);

        // Assert
        book.AverageRating.ShouldBe(5);
        book.TotalReviews.ShouldBe(1);

        // Add another rating and check the average
        book.AddRating(3);
        book.AverageRating.ShouldBe(4); // (5 + 3) / 2 = 4
        book.TotalReviews.ShouldBe(2);
    }

    [Test]
    public void GivenExistingRating_WhenRemovingRating_ThenShouldUpdateAverageRating()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "test.jpg",
            19.99m,
            15.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        // Add some ratings
        book.AddRating(5);
        book.AddRating(3);

        // Act
        book.RemoveRating(5);

        // Assert
        book.AverageRating.ShouldBe(3);
        book.TotalReviews.ShouldBe(1);
        book.DomainEvents.ShouldContain(e => e is BookChangedEvent);
    }

    [Test]
    public void GivenBookWithOnlyOneRating_WhenRemovingRating_ThenShouldResetRatingToZero()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "test.jpg",
            19.99m,
            15.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        // Add one rating
        book.AddRating(5);

        // Act
        book.RemoveRating(5);

        // Assert
        book.AverageRating.ShouldBe(0);
        book.TotalReviews.ShouldBe(0);
        book.DomainEvents.ShouldContain(e => e is BookChangedEvent);
    }

    [Test]
    public void GivenBookWithNoRatings_WhenRemovingRating_ThenShouldKeepRatingAtZero()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "test.jpg",
            19.99m,
            15.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        // Act
        book.RemoveRating(3);

        // Assert
        book.AverageRating.ShouldBe(0);
        book.TotalReviews.ShouldBe(0);
        book.DomainEvents.ShouldContain(e => e is BookChangedEvent);
    }

    [Test]
    public void GivenBookWithMultipleRatings_WhenRemovingRating_ThenShouldCalculateCorrectAverage()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "test.jpg",
            19.99m,
            15.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        // Add multiple ratings: 5, 4, 3, 2, 1 (average = 3.0)
        book.AddRating(5);
        book.AddRating(4);
        book.AddRating(3);
        book.AddRating(2);
        book.AddRating(1);

        // Act - Remove rating of 5
        book.RemoveRating(5);

        // Assert - New average should be (4+3+2+1)/4 = 2.5
        book.AverageRating.ShouldBe(2.5);
        book.TotalReviews.ShouldBe(4);
        book.DomainEvents.ShouldContain(e => e is BookChangedEvent);
    }

    [Test]
    public void GivenActiveBook_WhenDeleting_ThenShouldMarkAsDeleted()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "test.jpg",
            19.99m,
            15.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        // Act
        book.Delete();

        // Assert
        book.IsDeleted.ShouldBeTrue();
    }

    [Test]
    public void GivenValidParameters_WhenSetMetadata_ThenShouldSetMetadataCorrectly()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "test.jpg",
            19.99m,
            15.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        const string description = "Test Description";
        var categoryId = Guid.CreateVersion7();
        var publisherId = Guid.CreateVersion7();
        Guid[] authorIds = [Guid.CreateVersion7(), Guid.CreateVersion7()];

        // Act
        book.SetMetadata(description, categoryId, publisherId, authorIds);

        // Assert
        book.Description.ShouldBe(description);
        book.CategoryId.ShouldBe(categoryId);
        book.PublisherId.ShouldBe(publisherId);
        book.BookAuthors.Count.ShouldBe(authorIds.Length + 1);

        foreach (var authorId in authorIds)
        {
            book.BookAuthors.ShouldContain(
                ba => ba.AuthorId == authorId,
                $"Book should contain author with ID {authorId}"
            );
        }
    }

    [Test]
    public void GivenBookAndBookAuthor_WhenInitialized_ThenNavigationPropertiesShouldBeSet()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "test.jpg",
            19.99m,
            15.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        var bookAuthor = new BookAuthor(Guid.CreateVersion7());

        // Act
        // Note: In a real scenario, EF Core would set these
        typeof(Book).GetProperty("Category")?.SetValue(book, new Category());
        typeof(Book).GetProperty("Publisher")?.SetValue(book, new Publisher());
        typeof(BookAuthor).GetProperty("Author")?.SetValue(bookAuthor, new Author());
        typeof(BookAuthor).GetProperty("Book")?.SetValue(bookAuthor, book);

        // Assert
        book.Category.ShouldNotBeNull();
        book.Publisher.ShouldNotBeNull();
        bookAuthor.Author.ShouldNotBeNull();
        bookAuthor.Book.ShouldNotBeNull();
        bookAuthor.Book.ShouldBe(book);
    }

    [Test]
    public void GivenValidParameters_WhenSettingMetadata_ThenShouldReturnSameInstance()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "test.jpg",
            19.99m,
            15.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        const string description = "New Description";
        var categoryId = Guid.CreateVersion7();
        var publisherId = Guid.CreateVersion7();
        Guid[] authorIds = [Guid.CreateVersion7()];

        // Act
        var result = book.SetMetadata(description, categoryId, publisherId, authorIds);

        // Assert
        result.ShouldBeSameAs(book);
    }

    [Test]
    public void GivenValidParameters_WhenUpdatingBook_ThenShouldReturnSameInstance()
    {
        // Arrange
        var book = new Book(
            "Original Name",
            "Original Description",
            "original.jpg",
            19.99m,
            15.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        // Act
        var result = book.Update(
            "New Name",
            "New Description",
            29.99m,
            24.99m,
            "new.jpg",
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        // Assert
        result.ShouldBeSameAs(book);
    }

    [Test]
    public void GivenRating_WhenAddingRating_ThenShouldReturnSameInstance()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "test.jpg",
            19.99m,
            15.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        // Act
        var result = book.AddRating(5);

        // Assert
        result.ShouldBeSameAs(book);
    }

    [Test]
    public void GivenRating_WhenRemovingRating_ThenShouldReturnSameInstance()
    {
        // Arrange
        var book = new Book(
            "Test Book",
            "Test Description",
            "test.jpg",
            19.99m,
            15.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        book.AddRating(5); // Add a rating first

        // Act
        var result = book.RemoveRating(5);

        // Assert
        result.ShouldBeSameAs(book);
    }
}
