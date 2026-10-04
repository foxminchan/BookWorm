using BookWorm.Chassis.Specification;
using BookWorm.Chassis.Specification.Evaluators;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class RecentOrdersSpecTests
{
    [Test]
    public void GivenMoreOrdersThanLimit_WhenEvaluatingSpec_ThenShouldReturnNewestOrders()
    {
        // Arrange
        var date = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        var oldest = new Order { CreatedAt = date };
        var newest = new Order { CreatedAt = date.AddDays(2) };
        var middle = new Order { CreatedAt = date.AddDays(1) };
        Order[] orders = [oldest, newest, middle];
        var spec = new RecentOrdersSpec(2);

        // Act
        var result = SpecificationEvaluator.Instance.GetQuery(orders.AsQueryable(), spec).ToArray();

        // Assert
        result.ShouldBe([newest, middle]);
        spec.AsNoTracking.ShouldBeTrue();
    }

    [Test]
    public void GivenMatchingTimestamps_WhenCreatingSpec_ThenShouldBreakTiesByDescendingId()
    {
        // Arrange
        var order = new Order
        {
            Id = OrderId.From(Guid.Parse("00000000-0000-0000-0000-000000000001")),
            CreatedAt = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
        };

        // Act
        var expressions = new RecentOrdersSpec(5).OrderExpressions.ToArray();

        // Assert
        expressions.Length.ShouldBe(2);
        expressions[0].OrderType.ShouldBe(OrderType.OrderByDescending);
        expressions[0].KeySelector.Compile()(order).ShouldBe(order.CreatedAt);
        expressions[1].OrderType.ShouldBe(OrderType.ThenByDescending);
        expressions[1].KeySelector.Compile()(order).ShouldBe(order.Id);
    }

    [Test]
    public void GivenZeroLimit_WhenEvaluatingSpec_ThenShouldReturnNoOrders()
    {
        // Arrange
        Order[] orders = [new()];
        var spec = new RecentOrdersSpec(0);

        // Act
        var result = SpecificationEvaluator.Instance.GetQuery(orders.AsQueryable(), spec).ToArray();

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public void GivenFewerOrdersThanLimit_WhenEvaluatingSpec_ThenShouldReturnAllOrders()
    {
        // Arrange
        var order = new Order();
        Order[] orders = [order];
        var spec = new RecentOrdersSpec(5);

        // Act
        var result = SpecificationEvaluator.Instance.GetQuery(orders.AsQueryable(), spec).ToArray();

        // Assert
        result.ShouldBe([order]);
    }
}
