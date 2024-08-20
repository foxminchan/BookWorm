using BookWorm.Ordering.Domain.BuyerAggregate;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class BuyerAggregateTests
{
    [Fact]
    public void GivenValidConstructorArguments_ShouldInitializePropertiesCorrectly_WhenCreatingBuyer()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string name = "John Doe";
        var address = new Address("123 Main St", "City", "Province");

        // Act
        var buyer = new Buyer(id, name, address);

        // Assert
        buyer.Id.Should().Be(id);
        buyer.Name.Should().Be(name);
        buyer.Address.Should().Be(address);
        buyer.Orders.Should().BeEmpty();
    }

    [Fact]
    public void GivenDefaultGuid_ShouldThrowArgumentException_WhenCreatingBuyer()
    {
        // Arrange
        var id = Guid.Empty;
        const string name = "John Doe";
        var address = new Address("123 Main St", "City", "Province");

        // Act
        Func<Buyer> act = () => new(id, name, address);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenNullName_ShouldThrowArgumentNullException_WhenCreatingBuyer()
    {
        // Arrange
        var id = Guid.NewGuid();
        var address = new Address("123 Main St", "City", "Province");

        // Act
        Func<Buyer> act = () => new(id, null!, address);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenNullAddress_ShouldThrowArgumentNullException_WhenCreatingBuyer()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string name = "John Doe";

        // Act
        Func<Buyer> act = () => new(id, name, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenNewAddress_ShouldUpdateAddressProperty_WhenUpdatingAddress()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string name = "John Doe";
        var initialAddress = new Address("123 Main St", "City", "Province");
        var buyer = new Buyer(id, name, initialAddress);

        var newAddress = new Address("456 Elm St", "New City", "New Province");

        // Act
        buyer.UpdateAddress(newAddress);

        // Assert
        buyer.Address.Should().Be(newAddress);
    }

    [Fact]
    public void GivenNullAddress_ShouldThrowArgumentNullException_WhenUpdatingAddress()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string name = "John Doe";
        var initialAddress = new Address("123 Main St", "City", "Province");
        var buyer = new Buyer(id, name, initialAddress);

        // Act
        var act = () => buyer.UpdateAddress(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [CombinatorialData]
    public void GivenInvalidAddress_ShouldThrowArgumentException_WhenCreatingAddress(
        [CombinatorialValues(null, "")] string street,
        [CombinatorialValues(null, "")] string city,
        [CombinatorialValues(null, "")] string province)
    {
        // Act
        Func<Address> act = () => new(street, city, province);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
