using BookWorm.Catalog.Features.Books.Create;
using BookWorm.Catalog.Infrastructure.Blob;
using Microsoft.AspNetCore.Http;

namespace BookWorm.Catalog.UnitTests.Features.Books.Create;

public sealed class CreateBookPreProcessorTests
{
    [Test]
    public async Task GivenValidImageFile_WhenProcessing_ThenShouldUploadFileAndSetImageName()
    {
        // Arrange
        const string imageName = "an-image.jpg";
        var mockBlobService = new Mock<IBlobService>();
        mockBlobService
            .Setup(s => s.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageName);

        var mockFile = new Mock<IFormFile>();
        var command = new CreateBookCommand(
            "Test Book",
            "Test Description",
            mockFile.Object,
            29.99m,
            19.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        var handler = new CreateBookPreProcessor(mockBlobService.Object);

        // Act
        await handler.Handle(command, (_, _) => new(Guid.Empty), CancellationToken.None);

        // Assert
        command.ImageName.ShouldBe(imageName);
        mockBlobService.Verify(
            s => s.UploadFileAsync(mockFile.Object, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNullImage_WhenProcessing_ThenShouldNotCallBlobService()
    {
        // Arrange
        var mockBlobService = new Mock<IBlobService>();
        var command = new CreateBookCommand(
            "Test Book",
            "Test Description",
            null,
            29.99m,
            19.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        var handler = new CreateBookPreProcessor(mockBlobService.Object);

        // Act
        await handler.Handle(command, (_, _) => new(Guid.Empty), CancellationToken.None);

        // Assert
        command.ImageName.ShouldBeNull();
        mockBlobService.Verify(
            s => s.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
