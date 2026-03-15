using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class OrderFilterSpecTests
{
    [Test]
    public void GivenNoFilters_WhenCreatingSpec_ThenShouldSetAsNoTracking()
    {
        // Act
        var spec = new OrderFilterSpec();

        // Assert
        spec.AsNoTracking.ShouldBeTrue();
    }

    [Test]
    public void GivenStatusFilter_WhenCreatingSpec_ThenShouldApplyStatusFilter()
    {
        // Act
        var spec = new OrderFilterSpec(Status.New);

        // Assert
        spec.AsNoTracking.ShouldBeTrue();
        spec.WhereExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenBuyerIdFilter_WhenCreatingSpec_ThenShouldApplyBuyerFilter()
    {
        // Arrange
        var buyerId = Guid.CreateVersion7();

        // Act
        var spec = new OrderFilterSpec(buyerId: buyerId);

        // Assert
        spec.AsNoTracking.ShouldBeTrue();
        spec.WhereExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenStatusAndBuyerIdFilters_WhenCreatingSpec_ThenShouldApplyBothFilters()
    {
        // Arrange
        var buyerId = Guid.CreateVersion7();

        // Act
        var spec = new OrderFilterSpec(Status.Completed, buyerId);

        // Assert
        spec.AsNoTracking.ShouldBeTrue();
        spec.WhereExpressions.Count().ShouldBe(2);
    }

    [Test]
    public void GivenPagination_WhenCreatingSpec_ThenShouldApplyPaging()
    {
        // Arrange
        const int pageIndex = 2;
        const int pageSize = 10;

        // Act
        var spec = new OrderFilterSpec(pageIndex, pageSize);

        // Assert
        spec.Skip.ShouldBe((pageIndex - 1) * pageSize);
        spec.Take.ShouldBe(pageSize);
    }

    [Test]
    public void GivenPaginationWithFilters_WhenCreatingSpec_ThenShouldApplyPagingAndFilters()
    {
        // Arrange
        const int pageIndex = 1;
        const int pageSize = 5;
        var buyerId = Guid.CreateVersion7();

        // Act
        var spec = new OrderFilterSpec(pageIndex, pageSize, Status.New, buyerId);

        // Assert
        spec.Skip.ShouldBe(0);
        spec.Take.ShouldBe(pageSize);
        spec.WhereExpressions.ShouldNotBeEmpty();
        spec.OrderExpressions.ShouldNotBeEmpty();
    }

    [Test]
    public void GivenOrderIdAndStatus_WhenCreatingSpec_ThenShouldApplyFilter()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();

        // Act
        var spec = new OrderFilterSpec(orderId, Status.Completed);

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
        spec.AsTracking.ShouldBeTrue();
    }

    [Test]
    public void GivenOrderIdAndBuyerId_WhenCreatingSpec_ThenShouldApplyFilter()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var buyerId = Guid.CreateVersion7();

        // Act
        var spec = new OrderFilterSpec(orderId, buyerId);

        // Assert
        spec.WhereExpressions.ShouldNotBeEmpty();
        spec.AsTracking.ShouldBeTrue();
    }

    [Test]
    public void GivenFirstPage_WhenCreatingSpec_ThenSkipShouldBeZero()
    {
        // Act
        var spec = new OrderFilterSpec(1, 10);

        // Assert
        spec.Skip.ShouldBe(0);
        spec.Take.ShouldBe(10);
    }

    [Test]
    public void GivenThirdPage_WhenCreatingSpec_ThenSkipShouldBeCalculated()
    {
        // Act
        var spec = new OrderFilterSpec(3, 20);

        // Assert
        spec.Skip.ShouldBe(40);
        spec.Take.ShouldBe(20);
    }
}
