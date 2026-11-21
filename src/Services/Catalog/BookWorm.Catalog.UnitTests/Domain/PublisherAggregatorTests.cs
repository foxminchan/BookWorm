using BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;
using BookWorm.Catalog.Domain.Events;
using BookWorm.Catalog.Domain.Exceptions;

namespace BookWorm.Catalog.UnitTests.Domain;

public sealed class PublisherAggregatorTests
{
    [Test]
    public void GivenValidName_WhenCreatingPublisher_ThenShouldInitializeCorrectly()
    {
        // Arrange
        const string name = "Test Publisher";

        // Act
        var publisher = new Publisher(name);

        // Assert
        publisher.Name.ShouldBe(name);
        publisher.DomainEvents.ShouldContain(e => e is PublisherCreatedEvent);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    public void GivenEmptyOrNullName_WhenCreatingPublisher_ThenShouldThrowException(string? name)
    {
        // Act
        Func<Publisher> act = () => new(name!);

        // Assert
        act.ShouldThrow<CatalogDomainException>();
    }

    [Test]
    public void GivenValidName_WhenUpdatingPublisherName_ThenShouldUpdateCorrectly()
    {
        // Arrange
        var publisher = new Publisher("Original Name");
        const string newName = "Updated Name";

        // Act
        publisher.UpdateName(newName);

        // Assert
        publisher.Name.ShouldBe(newName);
        publisher.DomainEvents.ShouldContain(e => e is PublisherUpdatedEvent);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    public void GivenEmptyOrNullName_WhenUpdatingPublisherName_ThenShouldThrowException(
        string? newName
    )
    {
        // Arrange
        var publisher = new Publisher("Original Name");

        // Act
        var act = () => publisher.UpdateName(newName!);

        // Assert
        act.ShouldThrow<CatalogDomainException>();
    }

    [Test]
    public void GivenValidName_WhenUpdatingPublisherName_ThenShouldReturnSameInstance()
    {
        // Arrange
        var publisher = new Publisher("Original Name");
        const string newName = "Updated Name";

        // Act
        var result = publisher.UpdateName(newName);

        // Assert
        result.ShouldBeSameAs(publisher);
    }

    [Test]
    public void GivenExistingPublisher_WhenRemoving_ThenShouldRegisterPublisherDeletedEvent()
    {
        // Arrange
        var publisher = new Publisher("Test Publisher");
        publisher.ClearDomainEvents();

        // Act
        var result = publisher.Remove();

        // Assert
        result.ShouldBeSameAs(publisher);
        publisher.DomainEvents.ShouldContain(e => e is PublisherDeletedEvent);
    }
}
