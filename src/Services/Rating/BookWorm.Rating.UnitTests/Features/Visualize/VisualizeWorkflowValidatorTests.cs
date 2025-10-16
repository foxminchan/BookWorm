using BookWorm.Rating.Features.Visualize;
using BookWorm.SharedKernel;
using FluentValidation.TestHelper;

namespace BookWorm.Rating.UnitTests.Features.Visualize;

public sealed class VisualizeWorkflowValidatorTests
{
    private readonly VisualizeWorkflowValidator _validator = new();

    [Test]
    public void GivenValidQueryWithMermaidType_WhenValidating_ThenShouldNotHaveValidationErrors()
    {
        // Arrange
        var query = new VisualizeWorkflowQuery();

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenValidQueryWithDotType_WhenValidating_ThenShouldNotHaveValidationErrors()
    {
        // Arrange
        var query = new VisualizeWorkflowQuery(Visualizations.Dot);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenDefaultQuery_WhenValidating_ThenShouldNotHaveValidationErrors()
    {
        // Arrange
        var query = new VisualizeWorkflowQuery();

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    [Arguments(-1)]
    [Arguments(2)]
    [Arguments(99)]
    [Arguments(255)]
    public void GivenInvalidEnumValue_WhenValidating_ThenShouldHaveValidationError(int invalidValue)
    {
        // Arrange
        var query = new VisualizeWorkflowQuery((Visualizations)invalidValue);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Test]
    public void GivenInvalidEnumValue_WhenValidating_ThenShouldHaveValidationErrorForType()
    {
        // Arrange
        var query = new VisualizeWorkflowQuery((Visualizations)100);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Test]
    [Arguments(Visualizations.Mermaid)]
    [Arguments(Visualizations.Dot)]
    public void GivenAllValidEnumValues_WhenValidating_ThenShouldNotHaveValidationErrors(
        Visualizations type
    )
    {
        // Arrange
        var query = new VisualizeWorkflowQuery(type);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenMinimumEnumValue_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var query = new VisualizeWorkflowQuery();

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Test]
    public void GivenMaximumValidEnumValue_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var query = new VisualizeWorkflowQuery((Visualizations)1);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Test]
    public void GivenEnumValueOutOfRange_WhenValidating_ThenShouldHaveValidationErrorWithCorrectMessage()
    {
        // Arrange
        const Visualizations invalidValue = (Visualizations)50;
        var query = new VisualizeWorkflowQuery(invalidValue);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type);
        result.Errors.ShouldContain(e => e.PropertyName == nameof(VisualizeWorkflowQuery.Type));
    }
}
