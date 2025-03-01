using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;
using BookWorm.Ordering.Domain.Exceptions;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class AddressValueObjectTests
{
    [Test]
    public void GivenValidAddressData_WhenCreatingAddress_ThenShouldCreateAddressSuccessfully()
    {
        // Arrange
        const string street = "123 Main St";
        const string city = "Seattle";
        const string province = "WA";

        // Act
        var address = new Address(street, city, province);

        // Assert
        address.Street.ShouldBe(street);
        address.City.ShouldBe(city);
        address.Province.ShouldBe(province);
    }

    [Test]
    public void GivenTwoIdenticalAddresses_WhenComparingEquality_ThenShouldBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Seattle", "WA");
        var address2 = new Address("123 Main St", "Seattle", "WA");

        // Act & Assert
        address1.Equals(address2).ShouldBeTrue();
    }

    [Test]
    public void GivenTwoDifferentAddresses_WhenComparingEquality_ThenShouldNotBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Seattle", "WA");
        var address2 = new Address("456 Second Ave", "Portland", "OR");

        // Act & Assert
        address1.Equals(address2).ShouldBeFalse();
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public void GivenNullOrEmptyStreet_WhenCreatingAddress_ThenShouldThrowOrderingDomainException(
        string? street
    )
    {
        // Arrange & Act
        Func<Address> act = () => new(street, "Seattle", "WA");

        // Assert
        var exception = act.ShouldThrow<OrderingDomainException>();
        exception.Message.ShouldBe("Street is required");
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public void GivenNullOrEmptyCity_WhenCreatingAddress_ThenShouldThrowOrderingDomainException(
        string? city
    )
    {
        // Arrange & Act
        Func<Address> act = () => new("123 Main St", city, "WA");

        // Assert
        var exception = act.ShouldThrow<OrderingDomainException>();
        exception.Message.ShouldBe("City is required");
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public void GivenNullOrEmptyProvince_WhenCreatingAddress_ThenShouldThrowOrderingDomainException(
        string? province
    )
    {
        // Arrange & Act
        Func<Address> act = () => new("123 Main St", "Seattle", province);

        // Assert
        var exception = act.ShouldThrow<OrderingDomainException>();
        exception.Message.ShouldBe("Province is required");
    }

    [Test]
    public void GivenAddress_WhenCallingToString_ThenShouldReturnFormattedAddress()
    {
        // Arrange
        var address = new Address("123 Main St", "Seattle", "WA");

        // Act
        var result = address.ToString();

        // Assert
        result.ShouldBe("123 Main St, Seattle, WA");
    }

    [Test]
    public void GivenTwoIdenticalAddresses_WhenComparingHashCodes_ThenShouldBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Seattle", "WA");
        var address2 = new Address("123 Main St", "Seattle", "WA");

        // Act & Assert
        address1.GetHashCode().ShouldBe(address2.GetHashCode());
    }

    [Test]
    public void GivenAddress_WhenCopied_ThenShouldReturnEquivalentButSeparateInstance()
    {
        // Arrange
        var original = new Address("123 Main St", "Seattle", "WA");

        // Act
        var copy = original.GetCopy() as Address;

        // Assert
        copy.ShouldNotBeNull();
        copy.ShouldBe(original); // Value equality
        ReferenceEquals(copy, original).ShouldBeFalse(); // Different instances
    }

    [Test]
    public void GivenAddressAndNullObject_WhenComparingEquality_ThenShouldNotBeEqual()
    {
        // Arrange
        var address = new Address("123 Main St", "Seattle", "WA");

        // Act & Assert
        address.Equals(null).ShouldBeFalse();
    }

    [Test]
    public void GivenAddressAndDifferentTypeObject_WhenComparingEquality_ThenShouldNotBeEqual()
    {
        // Arrange
        var address = new Address("123 Main St", "Seattle", "WA");
        var differentObject = new object();

        // Act & Assert
        address.Equals(differentObject).ShouldBeFalse();
    }
}
