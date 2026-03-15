using BookWorm.Notification.Domain.Models;
using BookWorm.SharedKernel.Helpers;

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
    public void GivenOrder_WhenCreated_ThenCreatedAtShouldDefaultToToday()
    {
        // Act
        var order = new Order(Guid.CreateVersion7(), "Test User", 50.00m, Status.New);

        // Assert
        order.CreatedAt.ShouldBe(DateOnly.FromDateTime(DateTimeHelper.UtcNow()));
    }

    [Test]
    public void GivenOrder_WhenCreatedAtIsExplicitlySet_ThenShouldUseProvidedValue()
    {
        // Arrange
        var customDate = new DateOnly(2025, 6, 15);

        // Act
        var order = new Order(Guid.CreateVersion7(), "Test User", 50.00m, Status.Completed)
        {
            CreatedAt = customDate,
        };

        // Assert
        order.CreatedAt.ShouldBe(customDate);
    }

    [Test]
    [Arguments(Status.New)]
    [Arguments(Status.Completed)]
    [Arguments(Status.Canceled)]
    internal async Task GivenOrder_WhenCreatedWithStatus_ThenStatusShouldMatch(Status status)
    {
        // Act
        var order = new Order(Guid.CreateVersion7(), "Test User", 10.00m, status);

        // Assert
        await Assert.That(order.Status).IsEqualTo(status);
    }

    [Test]
    public void GivenTwoOrdersWithSameValues_WhenCompared_ThenShouldBeEqual()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var date = new DateOnly(2025, 1, 1);

        var order1 = new Order(id, "User", 99.99m, Status.New) { CreatedAt = date };
        var order2 = new Order(id, "User", 99.99m, Status.New) { CreatedAt = date };

        // Act & Assert
        order1.ShouldBe(order2);
    }

    [Test]
    public void GivenTwoOrdersWithDifferentIds_WhenCompared_ThenShouldNotBeEqual()
    {
        // Arrange
        var order1 = new Order(Guid.CreateVersion7(), "User", 99.99m, Status.New);
        var order2 = new Order(Guid.CreateVersion7(), "User", 99.99m, Status.New);

        // Act & Assert
        order1.ShouldNotBe(order2);
    }
}
