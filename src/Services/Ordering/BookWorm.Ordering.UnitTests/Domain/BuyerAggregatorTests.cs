using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.Exceptions;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class BuyerAggregateTests
{
    [Test]
    public void GivenValidParameters_WhenCreatingBuyer_ThenShouldCreateBuyerCorrectly()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string name = "John Doe";
        const string street = "123 Main St";
        const string city = "Bookville";
        const string province = "Readland";

        // Act
        var buyer = new Buyer(id, name, street, city, province);

        // Assert
        buyer.Id.ShouldBe(id);
        buyer.Name.ShouldBe(name);
        buyer.Address.ShouldNotBeNull();
        buyer.Address!.Street.ShouldBe(street);
        buyer.Address!.City.ShouldBe(city);
        buyer.Address!.Province.ShouldBe(province);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    public void GivenInvalidName_WhenCreatingBuyer_ThenShouldThrowOrderingDomainException(
        string? name
    )
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string street = "123 Main St";
        const string city = "Bookville";
        const string province = "Readland";

        // Act & Assert
        Should
            .Throw<OrderingDomainException>(() => new Buyer(id, name!, street, city, province))
            .Message.ShouldBe("Name is required");
    }

    [Test]
    public void GivenValidParameters_WhenCreatingAddress_ThenShouldCreateAddressCorrectly()
    {
        // Arrange
        const string street = "123 Main St";
        const string city = "Bookville";
        const string province = "Readland";

        // Act
        var address = new Address(street, city, province);

        // Assert
        address.Street.ShouldBe(street);
        address.City.ShouldBe(city);
        address.Province.ShouldBe(province);
    }

    [Test]
    [Arguments(null, "Bookville", "Readland", "Street is required")]
    [Arguments("123 Main St", null, "Readland", "City is required")]
    [Arguments("123 Main St", "Bookville", null, "Province is required")]
    [Arguments("", "Bookville", "Readland", "Street is required")]
    [Arguments("123 Main St", "", "Readland", "City is required")]
    [Arguments("123 Main St", "Bookville", "", "Province is required")]
    [Arguments("  ", "Bookville", "Readland", "Street is required")]
    [Arguments("123 Main St", "  ", "Readland", "City is required")]
    [Arguments("123 Main St", "Bookville", "  ", "Province is required")]
    public void GivenInvalidParameters_WhenCreatingAddress_ThenShouldThrowOrderingDomainException(
        string? street,
        string? city,
        string? province,
        string expectedErrorMessage
    )
    {
        // Act & Assert
        Should
            .Throw<OrderingDomainException>(() => new Address(street!, city!, province!))
            .Message.ShouldBe(expectedErrorMessage);
    }

    [Test]
    public void GivenValidParameters_WhenUpdatingAddress_ThenShouldUpdateAddressCorrectly()
    {
        // Arrange
        var buyer = new Buyer(
            Guid.CreateVersion7(),
            "John Doe",
            "123 Main St",
            "Bookville",
            "Readland"
        );

        const string newStreet = "456 Book Avenue";
        const string newCity = "Novel City";
        const string newProvince = "Fiction State";

        // Act
        buyer.UpdateAddress(newStreet, newCity, newProvince);

        // Assert
        buyer.Address.ShouldNotBeNull();
        buyer.Address!.Street.ShouldBe(newStreet);
        buyer.Address!.City.ShouldBe(newCity);
        buyer.Address!.Province.ShouldBe(newProvince);
    }

    [Test]
    public void GivenAddress_WhenGettingFullAddress_ThenShouldReturnFormattedAddress()
    {
        // Arrange
        const string street = "123 Main St";
        const string city = "Bookville";
        const string province = "Readland";
        var buyer = new Buyer(Guid.CreateVersion7(), "John Doe", street, city, province);

        // Act
        var fullAddress = buyer.FullAddress;

        // Assert
        fullAddress.ShouldBe($"{street}, {city}, {province}");
    }

    [Test]
    public void GivenTwoIdenticalAddresses_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Bookville", "Readland");
        var address2 = new Address("123 Main St", "Bookville", "Readland");

        // Act & Assert
        address1.ShouldBe(address2);
        address1.GetHashCode().ShouldBe(address2.GetHashCode());
    }

    [Test]
    public void GivenTwoDifferentAddresses_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Bookville", "Readland");
        var address2 = new Address("456 Book Avenue", "Novel City", "Fiction State");

        // Act & Assert
        address1.ShouldNotBe(address2);
    }

    [Test]
    public void GivenNewBuyer_WhenGettingOrders_ThenShouldReturnEmptyReadOnlyCollection()
    {
        // Arrange
        var buyer = new Buyer(
            Guid.CreateVersion7(),
            "John Doe",
            "123 Main St",
            "Bookville",
            "Readland"
        );

        // Act
        var orders = buyer.Orders;

        // Assert
        orders.ShouldNotBeNull();
        orders.ShouldBeEmpty();
        orders.ShouldBeAssignableTo<IReadOnlyCollection<Order>>();
    }

    [Test]
    public void GivenValidParameters_WhenUpdatingAddress_ThenShouldReturnSameInstance()
    {
        // Arrange
        var buyer = new Buyer(
            Guid.CreateVersion7(),
            "John Doe",
            "123 Main St",
            "Bookville",
            "Readland"
        );

        const string newStreet = "456 Book Avenue";
        const string newCity = "Novel City";
        const string newProvince = "Fiction State";

        // Act
        var result = buyer.UpdateAddress(newStreet, newCity, newProvince);

        // Assert
        result.ShouldBeSameAs(buyer);
    }
}
