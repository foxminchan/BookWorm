using BookWorm.Catalog.Features.Books.Update;
using BookWorm.Catalog.Infrastructure.Blob;
using Microsoft.AspNetCore.Http;

namespace BookWorm.Catalog.UnitTests.Features.Books.Update;

public sealed class UpdateBookPreProcessorTests
{
    [Test]
    public async Task GivenFormFileExists_WhenPreProcessing_ThenShouldUploadFileAndSetImageName()
    {
        // Arrange
        var formFile = new Mock<IFormFile>();
        var blobService = new Mock<IBlobService>();
        const string expectedImageUrl = "image-url.jpg";

        blobService
            .Setup(x => x.UploadFileAsync(formFile.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedImageUrl);

        var command = new UpdateBookCommand(
            Guid.CreateVersion7(),
            "Test Book",
            "Description",
            formFile.Object,
            10.99m,
            null,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        var handler = new UpdateBookPreProcessor(blobService.Object);

        // Act
        await handler.Process(command, CancellationToken.None);

        // Assert
        command.ImageName.ShouldBe(expectedImageUrl);
        blobService.Verify(
            x => x.UploadFileAsync(formFile.Object, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNoFormFile_WhenPreProcessing_ThenShouldNotCallBlobService()
    {
        // Arrange
        var blobService = new Mock<IBlobService>();

        var command = new UpdateBookCommand(
            Guid.CreateVersion7(),
            "Test Book",
            "Description",
            null,
            10.99m,
            null,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );

        var handler = new UpdateBookPreProcessor(blobService.Object);

        // Act
        await handler.Process(command, CancellationToken.None);

        // Assert
        command.ImageName.ShouldBeNull();
        blobService.Verify(
            x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
