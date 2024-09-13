namespace BookWorm.Catalog.UnitTests.Domain;

public sealed class PublisherAggregatorTests
{
    [Fact]
    public void GivenValidName_ShouldInitializeCorrectly_WhenCreatingPublisher()
    {
        // Arrange
        const string name = "Test Publisher";

        // Act
        var publisher = new Publisher(name);

        // Assert
        publisher.Name.Should().Be(name);
    }

    [Fact]
    public void GivenNullName_ShouldThrowArgumentNullException_WhenCreatingPublisher()
    {
        // Arrange
        string? name = null;

        // Act
        Func<Publisher> act = () => new(name!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenValidName_ShouldUpdateNameCorrectly_WhenUpdatingPublisherName()
    {
        // Arrange
        var publisher = new Publisher("Original Name");
        const string newName = "Updated Name";

        // Act
        publisher.UpdateName(newName);

        // Assert
        publisher.Name.Should().Be(newName);
    }

    [Fact]
    public void GivenNullName_ShouldThrowArgumentNullException_WhenUpdatingPublisherName()
    {
        // Arrange
        var publisher = new Publisher("Original Name");
        string? newName = null;

        // Act
        var act = () => publisher.UpdateName(newName!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
