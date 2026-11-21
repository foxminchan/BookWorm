using BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;
using BookWorm.Catalog.Domain.Events;
using BookWorm.Catalog.Domain.Exceptions;

namespace BookWorm.Catalog.UnitTests.Domain;

public sealed class AuthorAggregatorTests
{
    [Test]
    public void GivenValidName_WhenCreatingAuthor_ThenShouldInitializeCorrectly()
    {
        // Arrange
        const string name = "Test Author";

        // Act
        var author = new Author(name);

        // Assert
        author.Name.ShouldBe(name);
        author.BookAuthors.ShouldBeEmpty();
        author.DomainEvents.ShouldContain(e => e is AuthorCreatedEvent);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    public void GivenEmptyOrNullName_WhenCreatingAuthor_ThenShouldThrowException(string? name)
    {
        // Act
        Func<Author> act = () => new(name!);

        // Assert
        act.ShouldThrow<CatalogDomainException>();
    }

    [Test]
    public void GivenValidName_WhenUpdatingAuthorName_ThenShouldUpdateCorrectly()
    {
        // Arrange
        var author = new Author("Original Name");
        const string newName = "Updated Name";

        // Act
        author.UpdateName(newName);

        // Assert
        author.Name.ShouldBe(newName);
        author.DomainEvents.ShouldContain(e => e is AuthorUpdatedEvent);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    public void GivenEmptyOrNullName_WhenUpdatingAuthorName_ThenShouldThrowException(
        string? newName
    )
    {
        // Arrange
        var author = new Author("Original Name");

        // Act
        var act = () => author.UpdateName(newName!);

        // Assert
        act.ShouldThrow<CatalogDomainException>();
    }

    [Test]
    public void GivenValidName_WhenUpdatingAuthorName_ThenShouldReturnSameInstance()
    {
        // Arrange
        var author = new Author("Original Name");
        const string newName = "Updated Name";

        // Act
        var result = author.UpdateName(newName);

        // Assert
        result.ShouldBeSameAs(author);
    }

    [Test]
    public void GivenExistingAuthor_WhenRemoving_ThenShouldRegisterAuthorDeletedEvent()
    {
        // Arrange
        var author = new Author("Test Author");
        author.ClearDomainEvents();

        // Act
        var result = author.Remove();

        // Assert
        result.ShouldBeSameAs(author);
        author.DomainEvents.ShouldContain(e => e is AuthorDeletedEvent);
    }
}
