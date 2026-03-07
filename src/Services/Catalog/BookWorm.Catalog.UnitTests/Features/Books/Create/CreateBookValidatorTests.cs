using System.Net.Mime;
using BookWorm.Catalog.Features.Books.Create;
using BookWorm.Constants.Core;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;

namespace BookWorm.Catalog.UnitTests.Features.Books.Create;

public sealed class CreateBookValidatorTests
{
    private CreateBookValidator _validator = null!;

    [Before(Test)]
    public void SetUp()
    {
        _validator = new();
    }

    [Test]
    public void GivenEmptyName_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand(string.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNameExceedsMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand(new('a', DataSchemaLength.Medium + 1));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenDescriptionExceedsMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand(description: new('a', DataSchemaLength.SuperLarge + 1));

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
        var command = CreateValidCommand(price: price);

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
        var command = CreateValidCommand(priceSale: priceSale);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PriceSale);
    }

    [Test]
    public void GivenPriceSaleGreaterThanPrice_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand(price: 10, priceSale: 15);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PriceSale);
    }

    [Test]
    public void GivenEmptyCategoryId_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand(categoryId: Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Test]
    public void GivenEmptyPublisherId_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand(publisherId: Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PublisherId);
    }

    [Test]
    public void GivenEmptyAuthorIds_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand(authorIds: []);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AuthorIds);
    }

    [Test]
    public void GivenImageSizeExceedsMaximum_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var mockFile = CreateMockFile(2 * 1048576, MediaTypeNames.Image.Jpeg);
        var command = CreateValidCommand(image: mockFile.Object);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Image);
    }

    [Test]
    public void GivenEmptyImageFile_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var mockFile = CreateMockFile(0, MediaTypeNames.Image.Jpeg);
        var command = CreateValidCommand(image: mockFile.Object);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Image);
    }

    [Test]
    [Arguments(MediaTypeNames.Application.Pdf)]
    [Arguments(MediaTypeNames.Text.Plain)]
    public void GivenInvalidImageContentType_WhenValidating_ThenShouldHaveValidationError(
        string contentType
    )
    {
        // Arrange
        var mockFile = CreateMockFile(1000, contentType);
        var command = CreateValidCommand(image: mockFile.Object);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Image);
    }

    [Test]
    public void GivenExtensionMismatchingContentType_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange - JPEG content type but .png extension
        var mockFile = CreateMockFile(1000, MediaTypeNames.Image.Jpeg, "test.png");
        var command = CreateValidCommand(image: mockFile.Object);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Image);
    }

    [Test]
    [Arguments(MediaTypeNames.Image.Jpeg, "test.jpg")]
    [Arguments(MediaTypeNames.Image.Jpeg, "test.jpeg")]
    [Arguments(MediaTypeNames.Image.Png, "test.png")]
    [Arguments(MediaTypeNames.Image.Webp, "test.webp")]
    public void GivenValidImageFile_WhenValidating_ThenShouldNotHaveValidationErrors(
        string contentType,
        string fileName
    )
    {
        // Arrange
        var mockFile = CreateMockFile(1000, contentType, fileName);
        var command = CreateValidCommand(image: mockFile.Object);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static CreateBookCommand CreateValidCommand(
        string name = "Test Book",
        string description = "Test Description",
        IFormFile? image = null,
        decimal price = 29.99m,
        decimal? priceSale = 19.99m,
        Guid? categoryId = null,
        Guid? publisherId = null,
        Guid[]? authorIds = null
    )
    {
        return new()
        {
            Name = name,
            Description = description,
            Image = image,
            Price = price,
            PriceSale = priceSale,
            CategoryId = categoryId ?? Guid.CreateVersion7(),
            PublisherId = publisherId ?? Guid.CreateVersion7(),
            AuthorIds = authorIds ?? [Guid.CreateVersion7(), Guid.CreateVersion7()],
        };
    }

    private static Mock<IFormFile> CreateMockFile(
        long length,
        string contentType,
        string fileName = "test.jpg"
    )
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        mockFile.Setup(f => f.FileName).Returns(fileName);
        return mockFile;
    }
}
