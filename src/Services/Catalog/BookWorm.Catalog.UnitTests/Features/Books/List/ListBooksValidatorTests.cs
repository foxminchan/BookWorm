using BookWorm.Catalog.Features.Books.List;
using FluentValidation.TestHelper;

namespace BookWorm.Catalog.UnitTests.Features.Books.List;

public sealed class ListBooksValidatorTests
{
    private readonly ListBooksValidator _validator = new();

    [Test]
    public void GivenValidQuery_WhenValidating_ThenShouldNotHaveValidationErrors()
    {
        // Arrange
        var query = new ListBooksQuery(1, 10);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenZeroPageIndex_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var query = new ListBooksQuery(0, 10);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PageIndex)
            .WithErrorMessage("'Page Index' must be greater than '0'.");
    }

    [Test]
    public void GivenNegativePageIndex_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var query = new ListBooksQuery(-1, 10);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PageIndex)
            .WithErrorMessage("'Page Index' must be greater than '0'.");
    }

    [Test]
    public void GivenZeroPageSize_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var query = new ListBooksQuery(1, 0);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("'Page Size' must be greater than '0'.");
    }

    [Test]
    public void GivenNegativePageSize_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var query = new ListBooksQuery(1, -1);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("'Page Size' must be greater than '0'.");
    }

    [Test]
    public void GivenInvalidPageIndexAndPageSize_WhenValidating_ThenShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var query = new ListBooksQuery(0, 0);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageIndex);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
        result.Errors.Count.ShouldBe(2);
    }
}
