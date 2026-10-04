using BookWorm.Chassis.Specification.Evaluators;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class OrderDateRangeSpecTests
{
    [Test]
    public void GivenDateRange_WhenEvaluatingSpec_ThenShouldIncludeStartAndExcludeEnd()
    {
        // Arrange
        var start = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1);
        var beforeStart = new Order { CreatedAt = start.AddTicks(-1) };
        var atStart = new Order { CreatedAt = start };
        var withinRange = new Order { CreatedAt = start.AddHours(12) };
        var beforeEnd = new Order { CreatedAt = end.AddTicks(-1) };
        var atEnd = new Order { CreatedAt = end };
        var afterEnd = new Order { CreatedAt = end.AddTicks(1) };
        Order[] orders = [beforeStart, atStart, withinRange, beforeEnd, atEnd, afterEnd];
        var spec = new OrderDateRangeSpec(start, end);

        // Act
        var result = SpecificationEvaluator.Instance.GetQuery(orders.AsQueryable(), spec).ToArray();

        // Assert
        result.ShouldBe([atStart, withinRange, beforeEnd]);
        spec.AsNoTracking.ShouldBeTrue();
    }

    [Test]
    public void GivenEmptyDateRange_WhenEvaluatingSpec_ThenShouldReturnNoOrders()
    {
        // Arrange
        var date = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        Order[] orders = [new() { CreatedAt = date }];
        var spec = new OrderDateRangeSpec(date, date);

        // Act
        var result = SpecificationEvaluator.Instance.GetQuery(orders.AsQueryable(), spec).ToArray();

        // Assert
        result.ShouldBeEmpty();
    }
}
