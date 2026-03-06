using System.Net.Mime;
using BookWorm.Catalog.Features.Books.Update;
using BookWorm.Constants.Core;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;

namespace BookWorm.Catalog.UnitTests.Features.Books.Update;

public sealed class UpdateBookValidatorTests
{
    private const string ValidName = "Test Book";
    private const string ValidDescription = "Test Description";
    private const decimal ValidPrice = 29.99m;
    private const decimal ValidPriceSale = 19.99m;
    private readonly Guid[] _validAuthorIds = [Guid.CreateVersion7(), Guid.CreateVersion7()];
    private readonly Guid _validCategoryId = Guid.CreateVersion7();
    private readonly Guid _validId = Guid.CreateVersion7();
    private readonly Guid _validPublisherId = Guid.CreateVersion7();
    private UpdateBookValidator _validator = null!;

    [Before(Test)]
    public void Setup()
    {
        _validator = new();
    }

    [Test]
    public void GivenValidCommand_WhenValidating_ThenShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new UpdateBookCommand(
            _validId,
            ValidName,
            ValidDescription,
            null,
            ValidPrice,
            ValidPriceSale,
            _validCategoryId,
            _validPublisherId,
            _validAuthorIds
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
        var command = new UpdateBookCommand(
            Guid.Empty,
            ValidName,
            ValidDescription,
            null,
            ValidPrice,
            ValidPriceSale,
            _validCategoryId,
            _validPublisherId,
            _validAuthorIds
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Test]
    public void GivenEmptyName_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateBookCommand(
            _validId,
            string.Empty,
            ValidDescription,
            null,
            ValidPrice,
            ValidPriceSale,
            _validCategoryId,
            _validPublisherId,
            _validAuthorIds
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNameExceedingMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('A', DataSchemaLength.Medium + 1);
        var command = new UpdateBookCommand(
            _validId,
            longName,
            ValidDescription,
            null,
            ValidPrice,
            ValidPriceSale,
            _validCategoryId,
            _validPublisherId,
            _validAuthorIds
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenDescriptionExceedingMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var longDescription = new string('A', DataSchemaLength.SuperLarge + 1);
        var command = new UpdateBookCommand(
            _validId,
            ValidName,
            longDescription,
            null,
            ValidPrice,
            ValidPriceSale,
            _validCategoryId,
            _validPublisherId,
            _validAuthorIds
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void GivenZeroPrice_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateBookCommand(
            _validId,
            ValidName,
            ValidDescription,
            null,
            0m,
            ValidPriceSale,
            _validCategoryId,
            _validPublisherId,
            _validAuthorIds
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Test]
    public void GivenNegativePrice_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateBookCommand(
            _validId,
            ValidName,
            ValidDescription,
            null,
            -10m,
            ValidPriceSale,
            _validCategoryId,
            _validPublisherId,
            _validAuthorIds
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Test]
    public void GivenZeroPriceSale_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateBookCommand(
            _validId,
            ValidName,
            ValidDescription,
            null,
            ValidPrice,
            0m,
            _validCategoryId,
            _validPublisherId,
            _validAuthorIds
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PriceSale);
    }

    [Test]
    public void GivenPriceSaleGreaterThanPrice_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateBookCommand(
            _validId,
            ValidName,
            ValidDescription,
            null,
            10m,
            15m,
            _validCategoryId,
            _validPublisherId,
            _validAuthorIds
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PriceSale);
    }

    [Test]
    public void GivenEmptyCategoryId_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateBookCommand(
            _validId,
            ValidName,
            ValidDescription,
            null,
            ValidPrice,
            ValidPriceSale,
            Guid.Empty,
            _validPublisherId,
            _validAuthorIds
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Test]
    public void GivenEmptyPublisherId_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateBookCommand(
            _validId,
            ValidName,
            ValidDescription,
            null,
            ValidPrice,
            ValidPriceSale,
            _validCategoryId,
            Guid.Empty,
            _validAuthorIds
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PublisherId);
    }

    [Test]
    public void GivenEmptyAuthorIds_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateBookCommand(
            _validId,
            ValidName,
            ValidDescription,
            null,
            ValidPrice,
            ValidPriceSale,
            _validCategoryId,
            _validPublisherId,
            []
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AuthorIds);
    }

    [Test]
    public void GivenImageExceedingMaxSize_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1048577); // 1 byte over limit
        mockFile.Setup(f => f.ContentType).Returns(MediaTypeNames.Image.Jpeg);
        mockFile.Setup(f => f.FileName).Returns("test.jpg");

        var command = new UpdateBookCommand(
            _validId,
            ValidName,
            ValidDescription,
            mockFile.Object,
            ValidPrice,
            ValidPriceSale,
            _validCategoryId,
            _validPublisherId,
            _validAuthorIds
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Image);
    }

    [Test]
    public void GivenInvalidImageContentType_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1000); // Valid size
        mockFile.Setup(f => f.ContentType).Returns(MediaTypeNames.Application.Pdf); // Invalid content type
        mockFile.Setup(f => f.FileName).Returns("test.pdf");

        var command = new UpdateBookCommand(
            _validId,
            ValidName,
            ValidDescription,
            mockFile.Object,
            ValidPrice,
            ValidPriceSale,
            _validCategoryId,
            _validPublisherId,
            _validAuthorIds
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Image);
    }

    [Test]
    public void GivenEmptyImageFile_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var mockFile = CreateMockFile(0, MediaTypeNames.Image.Jpeg, "test.jpg");

        var command = new UpdateBookCommand(
            _validId,
            ValidName,
            ValidDescription,
            mockFile.Object,
            ValidPrice,
            ValidPriceSale,
            _validCategoryId,
            _validPublisherId,
            _validAuthorIds
        );

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

        var command = new UpdateBookCommand(
            _validId,
            ValidName,
            ValidDescription,
            mockFile.Object,
            ValidPrice,
            ValidPriceSale,
            _validCategoryId,
            _validPublisherId,
            _validAuthorIds
        );

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

        var command = new UpdateBookCommand(
            _validId,
            ValidName,
            ValidDescription,
            mockFile.Object,
            ValidPrice,
            ValidPriceSale,
            _validCategoryId,
            _validPublisherId,
            _validAuthorIds
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Image);
    }

    private static Mock<IFormFile> CreateMockFile(long length, string contentType, string fileName)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        return mockFile;
    }
}
