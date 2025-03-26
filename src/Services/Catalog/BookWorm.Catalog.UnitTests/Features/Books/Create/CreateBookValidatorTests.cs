using System.Net.Mime;
using BookWorm.Catalog.Features.Books.Create;
using BookWorm.Constants;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;

namespace BookWorm.Catalog.UnitTests.Features.Books.Create;

public sealed class CreateBookValidatorTests
{
    private CreateBookValidator _validator = default!;

    [Before(Test)]
    public void SetUp()
    {
        _validator = new();
    }

    [Test]
    public void GivenEmptyName_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Name = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNameExceedsMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Name = new('a', DataSchemaLength.Medium + 1) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenDescriptionExceedsMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Description = new('a', DataSchemaLength.SuperLarge + 1) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    public void GivenInvalidPrice_WhenValidating_ThenShouldHaveValidationError(decimal price)
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Price = price };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    public void GivenInvalidPriceSale_WhenValidating_ThenShouldHaveValidationError(
        decimal priceSale
    )
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { PriceSale = priceSale };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PriceSale);
    }

    [Test]
    public void GivenPriceSaleGreaterThanPrice_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Price = 10, PriceSale = 15 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PriceSale);
    }

    [Test]
    public void GivenEmptyCategoryId_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { CategoryId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Test]
    public void GivenEmptyPublisherId_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { PublisherId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PublisherId);
    }

    [Test]
    public void GivenEmptyAuthorIds_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { AuthorIds = [] };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AuthorIds);
    }

    [Test]
    public void GivenImageSizeExceedsMaximum_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand();
        var mockFile = CreateMockFile(2 * 1048576, MediaTypeNames.Image.Jpeg);
        command = command with { Image = mockFile.Object };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Image!.Length);
    }

    [Test]
    [Arguments(MediaTypeNames.Application.Pdf)]
    [Arguments(MediaTypeNames.Text.Plain)]
    public void GivenInvalidImageContentType_WhenValidating_ThenShouldHaveValidationError(
        string contentType
    )
    {
        // Arrange
        var command = CreateValidCommand();
        var mockFile = CreateMockFile(1000, contentType);
        command = command with { Image = mockFile.Object };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Image!.ContentType);
    }

    [Test]
    public void GivenValidCommand_WhenValidating_ThenShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = CreateValidCommand();
        var mockFile = CreateMockFile(1000, MediaTypeNames.Image.Jpeg);
        command = command with { Image = mockFile.Object };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static CreateBookCommand CreateValidCommand()
    {
        return new(
            "Test Book",
            "Test Description",
            null,
            29.99m,
            19.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7(), Guid.CreateVersion7()]
        );
    }

    private static Mock<IFormFile> CreateMockFile(long length, string contentType)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        return mockFile;
    }
}
