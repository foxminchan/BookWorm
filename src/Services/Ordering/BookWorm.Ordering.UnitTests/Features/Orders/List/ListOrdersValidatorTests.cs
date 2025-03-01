using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Features.Orders.List;
using FluentValidation.TestHelper;

namespace BookWorm.Ordering.UnitTests.Features.Orders.List;

public sealed class ListOrdersValidatorTests
{
    private readonly ListOrdersValidator _validator = new();

    [Test]
    public void GivenValidQuery_WhenValidating_ThenShouldNotHaveErrors()
    {
        // Arrange
        var query = new ListOrdersQuery(1, 10, Status.Completed, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenDefaultQuery_WhenValidating_ThenShouldNotHaveErrors()
    {
        // Arrange
        var query = new ListOrdersQuery();

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(-100)]
    public void GivenInvalidPageIndex_WhenValidating_ThenShouldHaveError(int pageIndex)
    {
        // Arrange
        var query = new ListOrdersQuery(pageIndex, 10);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PageIndex)
            .WithErrorMessage("'Page Index' must be greater than '0'.");
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(-100)]
    public void GivenInvalidPageSize_WhenValidating_ThenShouldHaveError(int pageSize)
    {
        // Arrange
        var query = new ListOrdersQuery(1, pageSize);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("'Page Size' must be greater than '0'.");
    }

    [Test]
    public void GivenInvalidStatusValue_WhenValidating_ThenShouldHaveError()
    {
        // Arrange
        // Cast an invalid value to Status enum
        const Status invalidStatus = (Status)231;
        var query = new ListOrdersQuery(1, 10, invalidStatus);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("'Status' has a range of values which does not include '231'.");
    }
}
