namespace BookWorm.Catalog.UnitTests.Domain;

public sealed class CategoryAggregatorTests
{
    [Fact]
    public void GivenValidName_ShouldInitializeCorrectly_WhenCreatingCategory()
    {
        // Arrange
        const string name = "Test Category";

        // Act
        var category = new Category(name);

        // Assert
        category.Name.Should().Be(name);
    }

    [Fact]
    public void GivenNullName_ShouldThrowArgumentNullException_WhenCreatingCategory()
    {
        // Arrange
        string? name = null;

        // Act
        Func<Category> act = () => new(name!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenValidName_ShouldUpdateNameCorrectly_WhenUpdatingCategoryName()
    {
        // Arrange
        var category = new Category("Original Name");
        const string newName = "Updated Name";

        // Act
        category.UpdateName(newName);

        // Assert
        category.Name.Should().Be(newName);
    }

    [Fact]
    public void GivenNullName_ShouldThrowArgumentNullException_WhenUpdatingCategoryName()
    {
        // Arrange
        var category = new Category("Original Name");
        string? newName = null;

        // Act
        var act = () => category.UpdateName(newName!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
