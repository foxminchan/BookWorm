using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate.Specifications;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class BuyerFilterSpecTests
{
    [Test]
    public void GivenPagination_WhenCreatingSpec_ThenShouldSetAsNoTracking()
    {
        // Act
        var spec = new BuyerFilterSpec(1, 10);

        // Assert
        spec.AsNoTracking.ShouldBeTrue();
    }

    [Test]
    public void GivenFirstPage_WhenCreatingSpec_ThenSkipShouldBeZero()
    {
        // Act
        var spec = new BuyerFilterSpec(1, 10);

        // Assert
        spec.Skip.ShouldBe(0);
        spec.Take.ShouldBe(10);
    }

    [Test]
    public void GivenSecondPage_WhenCreatingSpec_ThenShouldCalculateSkip()
    {
        // Act
        var spec = new BuyerFilterSpec(2, 10);

        // Assert
        spec.Skip.ShouldBe(10);
        spec.Take.ShouldBe(10);
    }

    [Test]
    public void GivenThirdPage_WhenCreatingSpec_ThenShouldCalculateCorrectOffset()
    {
        // Arrange
        const int pageIndex = 3;
        const int pageSize = 25;

        // Act
        var spec = new BuyerFilterSpec(pageIndex, pageSize);

        // Assert
        spec.Skip.ShouldBe(50);
        spec.Take.ShouldBe(25);
    }
}
