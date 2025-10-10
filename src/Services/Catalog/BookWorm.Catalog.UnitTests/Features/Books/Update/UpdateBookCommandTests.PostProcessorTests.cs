using BookWorm.Catalog.Features.Books.Update;
using BookWorm.Catalog.Infrastructure.Blob;
using Mediator;

namespace BookWorm.Catalog.UnitTests.Features.Books.Update;

public sealed class UpdateBookPostProcessorTests
{
    [Test]
    public async Task GivenIsRemoveImageTrueAndImageNameExists_WhenProcessing_ThenShouldDeleteFile()
    {
        // Arrange
        var blobService = new Mock<IBlobService>();
        const string imageName = "image-to-delete.jpg";

        var command = new UpdateBookCommand(
            Guid.CreateVersion7(),
            "Test Book",
            "Description",
            null,
            10.99m,
            null,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()],
            true // IsRemoveImage = true
        )
        {
            ImageUrn = imageName,
        };

        var handler = new UpdateBookPostProcessor(blobService.Object);

        // Act
        await handler.Handle(command, (_, _) => new(Unit.Value), CancellationToken.None);

        // Assert
        blobService.Verify(
            x => x.DeleteFileAsync(imageName, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenIsRemoveImageFalse_WhenProcessing_ThenShouldNotCallBlobService()
    {
        // Arrange
        var blobService = new Mock<IBlobService>();
        const string imageName = "image.jpg";

        var command = new UpdateBookCommand(
            Guid.CreateVersion7(),
            "Test Book",
            "Description",
            null,
            10.99m,
            null,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()] // IsRemoveImage = false
        )
        {
            ImageUrn = imageName,
        };

        var handler = new UpdateBookPostProcessor(blobService.Object);

        // Act
        await handler.Handle(command, (_, _) => new(Unit.Value), CancellationToken.None);

        // Assert
        blobService.Verify(
            x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenIsRemoveImageTrueButNoImageName_WhenProcessing_ThenShouldNotCallBlobService()
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
            [Guid.CreateVersion7()],
            true // IsRemoveImage = true
        );

        var handler = new UpdateBookPostProcessor(blobService.Object);

        // Act
        await handler.Handle(command, (_, _) => new(Unit.Value), CancellationToken.None);

        // Assert
        blobService.Verify(
            x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
