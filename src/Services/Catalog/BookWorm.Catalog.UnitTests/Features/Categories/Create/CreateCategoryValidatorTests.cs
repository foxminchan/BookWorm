using BookWorm.Catalog.Features.Categories.Create;
using BookWorm.Constants;
using FluentValidation.TestHelper;

namespace BookWorm.Catalog.UnitTests.Features.Categories.Create;

public sealed class CreateCategoryValidatorTests
{
    private readonly CreateCategoryValidator _validator = new();

    [Test]
    public void GivenValidCommand_WhenValidating_ThenShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new CreateCategoryCommand("Fiction");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments(" ")]
    public void GivenEmptyOrNullName_WhenValidating_ThenShouldHaveValidationError(string? name)
    {
        // Arrange
        var command = new CreateCategoryCommand(name!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNameExceedingMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCategoryCommand(new('a', DataSchemaLength.Medium + 1));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
