using BookWorm.Rating.Features.List;
using FluentValidation.TestHelper;

namespace BookWorm.Rating.UnitTests.Features.List;

public sealed class ListFeedbacksValidatorTests
{
    private readonly ListFeedbacksValidator _validator = new();

    [Test]
    public void GivenValidQuery_WhenValidating_ThenShouldNotHaveValidationErrors()
    {
        // Arrange
        var query = new ListFeedbacksQuery(Guid.CreateVersion7(), 1, 10);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenNegativePageIndex_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var query = new ListFeedbacksQuery(Guid.CreateVersion7(), -1, 10);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.PageIndex);
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(-101)]
    public void GivenInvalidPageSize_WhenValidating_ThenShouldHaveValidationError(int pageSize)
    {
        // Arrange
        var query = new ListFeedbacksQuery(Guid.CreateVersion7(), 0, pageSize);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }

    [Test]
    public void GivenNullOrderBy_WhenValidating_ThenShouldUseDefaultValue()
    {
        // Arrange
        var query = new ListFeedbacksQuery(Guid.CreateVersion7(), 0, 10, default);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.OrderBy);
    }
}
