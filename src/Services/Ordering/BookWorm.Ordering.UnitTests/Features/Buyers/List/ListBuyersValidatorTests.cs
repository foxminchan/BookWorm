using BookWorm.Ordering.Features.Buyers.List;
using FluentValidation.TestHelper;

namespace BookWorm.Ordering.UnitTests.Features.Buyers.List;

public sealed class ListBuyersValidatorTests
{
    private readonly ListBuyersValidator _validator = new();

    [Test]
    public void GivenValidParameters_WhenValidatingListBuyersQuery_ThenShouldNotHaveValidationErrors()
    {
        // Arrange
        var query = new ListBuyersQuery(1, 10);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(-100)]
    public void GivenInvalidPageIndex_WhenValidatingListBuyersQuery_ThenShouldHaveValidationError(
        int invalidPageIndex
    )
    {
        // Arrange
        var query = new ListBuyersQuery(invalidPageIndex, 10);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.PageIndex)
            .WithErrorMessage("'Page Index' must be greater than '0'.");
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(-100)]
    public void GivenInvalidPageSize_WhenValidatingListBuyersQuery_ThenShouldHaveValidationError(
        int invalidPageSize
    )
    {
        // Arrange
        var query = new ListBuyersQuery(1, invalidPageSize);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("'Page Size' must be greater than '0'.");
    }

    [Test]
    public void GivenInvalidPageIndexAndPageSize_WhenValidatingListBuyersQuery_ThenShouldHaveBothValidationErrors()
    {
        // Arrange
        var query = new ListBuyersQuery(-1, -1);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.PageIndex);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
        result.Errors.Count.ShouldBe(2);
    }
}
