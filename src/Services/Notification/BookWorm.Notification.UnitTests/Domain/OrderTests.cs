using System.Globalization;
using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.UnitTests.Domain;

public sealed class OrderTests
{
    [Test]
    public void GivenOrder_WhenCreated_ThenPropertiesShouldBeSet()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string fullName = "Test User";
        const decimal totalMoney = 99.99m;
        const Status status = Status.New;

        // Act
        var order = new Order(id, fullName, totalMoney, status);

        // Assert
        order.Id.ShouldBe(id);
        order.FullName.ShouldBe(fullName);
        order.TotalMoney.ShouldBe(totalMoney);
        order.Status.ShouldBe(status);
    }

    [Test]
    public void GivenOrder_WhenTotalMoneyIsFormatted_ThenShouldUseCurrencyFormat()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), "Test User", 99.99m, Status.New);

        // Act
        var formattedMoney = order.TotalMoney.ToString("C", new CultureInfo("en-US"));

        // Assert
        formattedMoney.ShouldBe("$99.99");
    }
}
