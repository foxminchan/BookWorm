using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.UnitTests.Domain;

public sealed class AuthorAggregatorTests
{
    [Fact]
    public void GivenValidName_ShouldInitializeCorrectly_WhenCreatingAuthor()
    {
        // Arrange
        const string name = "Test Author";

        // Act
        var author = new Author(name);

        // Assert
        author.Name.Should().Be(name);
        author.BookAuthors.Should().BeEmpty();
    }

    [Fact]
    public void GivenNullName_ShouldThrowArgumentNullException_WhenCreatingAuthor()
    {
        // Arrange
        string? name = null;

        // Act
        Func<Author> act = () => new(name!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenValidName_ShouldUpdateNameCorrectly_WhenUpdatingAuthorName()
    {
        // Arrange
        var author = new Author("Original Name");
        const string newName = "Updated Name";

        // Act
        author.UpdateName(newName);

        // Assert
        author.Name.Should().Be(newName);
    }

    [Fact]
    public void GivenNullName_ShouldThrowArgumentNullException_WhenUpdatingAuthorName()
    {
        // Arrange
        var author = new Author("Original Name");
        string? newName = null;

        // Act
        Action act = () => author.UpdateName(newName!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
