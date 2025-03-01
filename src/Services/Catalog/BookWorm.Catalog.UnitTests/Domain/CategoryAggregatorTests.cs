using BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;
using BookWorm.Catalog.Domain.Exceptions;

namespace BookWorm.Catalog.UnitTests.Domain;

public class CategoryAggregatorTests
{
    [Test]
    public void GivenValidName_WhenCreatingCategory_ThenShouldInitializeCorrectly()
    {
        // Arrange
        const string name = "Test Category";

        // Act
        var category = new Category(name);

        // Assert
        category.Name.ShouldBe(name);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    public void GivenEmptyOrEmptyName_WhenCreatingCategory_ThenShouldThrowException(string? name)
    {
        // Act
        Func<Category> act = () => new(name!);

        // Assert
        act.ShouldThrow<CatalogDomainException>();
    }

    [Test]
    public void GivenValidName_WhenUpdatingCategoryName_ThenShouldUpdateCorrectly()
    {
        // Arrange
        var category = new Category("Original Name");
        const string newName = "Updated Name";

        // Act
        category.UpdateName(newName);

        // Assert
        category.Name.ShouldBe(newName);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    public void GivenEmptyOrNullName_WhenUpdatingCategoryName_ThenShouldThrowException(
        string? newName
    )
    {
        // Arrange
        var category = new Category("Original Name");

        // Act
        var act = () => category.UpdateName(newName!);

        // Assert
        act.ShouldThrow<CatalogDomainException>();
    }
}
