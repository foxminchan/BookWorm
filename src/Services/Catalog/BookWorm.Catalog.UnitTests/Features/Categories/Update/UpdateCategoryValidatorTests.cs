using BookWorm.Catalog.Features.Categories.Update;
using BookWorm.Constants.Core;
using FluentValidation.TestHelper;

namespace BookWorm.Catalog.UnitTests.Features.Categories.Update;

public sealed class UpdateCategoryValidatorTests
{
    private readonly Faker _faker = new();
    private readonly UpdateCategoryValidator _validator = new();

    [Test]
    public void GivenValidCommand_WhenValidating_ThenShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Guid.CreateVersion7(),
            _faker.Commerce.Categories(1)[0]
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenEmptyId_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateCategoryCommand(Guid.Empty, _faker.Commerce.Categories(1)[0]);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public void GivenEmptyName_WhenValidating_ThenShouldHaveValidationError(string? name)
    {
        // Arrange
        var command = new UpdateCategoryCommand(Guid.CreateVersion7(), name!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNameExceedingMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Guid.CreateVersion7(),
            new('a', DataSchemaLength.Medium + 1)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(
                $"The length of 'Name' must be {DataSchemaLength.Medium} characters or fewer. You entered {DataSchemaLength.Medium + 1} characters."
            );
    }
}
