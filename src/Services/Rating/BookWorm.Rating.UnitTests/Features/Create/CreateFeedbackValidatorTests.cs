using BookWorm.Constants.Core;
using BookWorm.Rating.Features.Create;
using FluentValidation.TestHelper;

namespace BookWorm.Rating.UnitTests.Features.Create;

public sealed class CreateFeedbackValidatorTests
{
    private readonly CreateFeedbackValidator _validator = new();

    [Test]
    public void GivenEmptyBookId_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateFeedbackCommand(Guid.Empty, "John", "Doe", "Great book!", 5);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BookId);
    }

    [Test]
    public void GivenValidBookId_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateFeedbackCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            "Great book!",
            5
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BookId);
    }

    [Test]
    public void GivenEmptyFirstName_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateFeedbackCommand(
            Guid.CreateVersion7(),
            null,
            "Doe",
            "Great book!",
            5
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Test]
    public void GivenEmptyLastName_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateFeedbackCommand(
            Guid.CreateVersion7(),
            "John",
            null,
            "Great book!",
            5
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Test]
    public void GivenTooLongComment_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var longComment = new string('A', DataSchemaLength.Max + 1);
        var command = new CreateFeedbackCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            longComment,
            5
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Comment);
    }

    [Test]
    public void GivenValidComment_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var validComment = new string('A', DataSchemaLength.Max);
        var command = new CreateFeedbackCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            validComment,
            5
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Comment);
    }

    [Test]
    [Arguments(-1)]
    [Arguments(6)]
    public void GivenRatingOutsideValidRange_WhenValidating_ThenShouldHaveValidationError(
        int invalidRating
    )
    {
        // Arrange
        var command = new CreateFeedbackCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            "Great book!",
            invalidRating
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Test]
    [Arguments(0)]
    [Arguments(3)]
    [Arguments(5)]
    public void GivenRatingWithinValidRange_WhenValidating_ThenShouldNotHaveValidationError(
        int validRating
    )
    {
        // Arrange
        var command = new CreateFeedbackCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            "Great book!",
            validRating
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Rating);
    }

    [Test]
    public void GivenValidCommand_WhenValidating_ThenShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new CreateFeedbackCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            "Great book!",
            5
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
