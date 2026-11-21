using BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;
using BookWorm.Catalog.Domain.Events;
using BookWorm.Catalog.Domain.Exceptions;

namespace BookWorm.Catalog.UnitTests.Domain;

public sealed class CategoryAggregatorTests
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
        category.DomainEvents.ShouldContain(e => e is CategoryCreatedEvent);
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
        category.DomainEvents.ShouldContain(e => e is CategoryUpdatedEvent);
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

    [Test]
    public void GivenValidName_WhenUpdatingCategoryName_ThenShouldReturnSameInstance()
    {
        // Arrange
        var category = new Category("Original Name");
        const string newName = "Updated Name";

        // Act
        var result = category.UpdateName(newName);

        // Assert
        result.ShouldBeSameAs(category);
    }

    [Test]
    public void GivenExistingCategory_WhenRemoving_ThenShouldRegisterCategoryDeletedEvent()
    {
        // Arrange
        var category = new Category("Test Category");
        category.ClearDomainEvents();

        // Act
        var result = category.Remove();

        // Assert
        result.ShouldBeSameAs(category);
        category.DomainEvents.ShouldContain(e => e is CategoryDeletedEvent);
    }
}
