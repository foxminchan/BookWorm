using BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;
using BookWorm.Catalog.Domain.Exceptions;

namespace BookWorm.Catalog.UnitTests.Domain;

public sealed class PublisherAggregatorTests
{
    [Test]
    public void GivenValidName_WhenCreatingPublisher_ShouldInitializeCorrectly()
    {
        // Arrange
        const string name = "Test Publisher";

        // Act
        var publisher = new Publisher(name);

        // Assert
        publisher.Name.ShouldBe(name);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    public void GivenEmptyOrNullName_WhenCreatingPublisher_ShouldThrowException(string? name)
    {
        // Act
        Func<Publisher> act = () => new(name!);

        // Assert
        act.ShouldThrow<CatalogDomainException>();
    }

    [Test]
    public void GivenValidName_WhenUpdatingPublisherName_ShouldUpdateCorrectly()
    {
        // Arrange
        var publisher = new Publisher("Original Name");
        const string newName = "Updated Name";

        // Act
        publisher.UpdateName(newName);

        // Assert
        publisher.Name.ShouldBe(newName);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    public void GivenEmptyOrNullName_WhenUpdatingPublisherName_ShouldThrowException(string? newName)
    {
        // Arrange
        var publisher = new Publisher("Original Name");

        // Act
        var act = () => publisher.UpdateName(newName!);

        // Assert
        act.ShouldThrow<CatalogDomainException>();
    }
}
