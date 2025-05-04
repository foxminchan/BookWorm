using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Features.Books.Update;
using BookWorm.Catalog.Infrastructure.Blob;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.Chassis.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BookWorm.Catalog.UnitTests.Features.Books.Update;

public sealed class UpdateBookCommandTests
{
    #region PreUpdateBookHandler Tests

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

        var handler = new PreUpdateBookHandler(blobService.Object);

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

        var handler = new PreUpdateBookHandler(blobService.Object);

        // Act
        await handler.Process(command, CancellationToken.None);

        // Assert
        command.ImageName.ShouldBeNull();
        blobService.Verify(
            x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion

    #region UpdateBookHandler Tests

    [Test]
    public async Task GivenValidCommand_WhenHandling_ThenShouldUpdateBook()
    {
        // Arrange
        var bookFaker = new BookFaker();
        var book = bookFaker.Generate(1)[0];

        var repository = new Mock<IBookRepository>();
        repository
            .Setup(x => x.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        repository
            .Setup(x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new UpdateBookCommand(
            book.Id,
            "Updated Name",
            "Updated Description",
            null,
            19.99m,
            9.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7(), Guid.CreateVersion7()]
        );

        var handler = new UpdateBookHandler(repository.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        book.Name.ShouldBe("Updated Name");
        book.Description.ShouldBe("Updated Description");
        book.Price!.OriginalPrice.ShouldBe(19.99m);
        book.Price.DiscountPrice.ShouldBe(9.99m);
        book.CategoryId.ShouldBe(command.CategoryId);
        book.PublisherId.ShouldBe(command.PublisherId);
        book.BookAuthors.Select(x => x.AuthorId).ShouldBe(command.AuthorIds);
        repository.Verify(
            x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public void GivenNonExistentBook_WhenHandling_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var repository = new Mock<IBookRepository>();
        repository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

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

        var handler = new UpdateBookHandler(repository.Object);

        // Act & Assert
        Should.Throw<NotFoundException>(async () =>
            await handler.Handle(command, CancellationToken.None)
        );

        repository.Verify(
            x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenIsRemoveImageTrue_WhenHandling_ThenShouldSetImageToNull()
    {
        // Arrange
        var bookFaker = new BookFaker();
        var book = bookFaker.Generate(1)[0];
        book.Update(
            book.Name!,
            book.Description,
            book.Price!.OriginalPrice,
            book.Price.DiscountPrice,
            "existing-image.jpg",
            (Guid)book.CategoryId!,
            (Guid)book.PublisherId!,
            [.. book.BookAuthors.Select(x => x.AuthorId)]
        );

        var repository = new Mock<IBookRepository>();
        repository
            .Setup(x => x.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        repository
            .Setup(x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new UpdateBookCommand(
            book.Id,
            book.Name!,
            book.Description!,
            null,
            book.Price!.OriginalPrice,
            book.Price.DiscountPrice,
            (Guid)book.CategoryId,
            (Guid)book.PublisherId,
            [.. book.BookAuthors.Select(x => x.AuthorId)],
            true // IsRemoveImage = true
        );

        var handler = new UpdateBookHandler(repository.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        book.Image.ShouldBeNull();
    }

    [Test]
    public async Task GivenIsRemoveImageFalseAndNewImage_WhenHandling_ThenShouldUpdateImage()
    {
        // Arrange
        var bookFaker = new BookFaker();
        var book = bookFaker.Generate(1)[0];
        book.Update(
            book.Name!,
            book.Description!,
            book.Price!.OriginalPrice,
            book.Price.DiscountPrice,
            "existing-image.jpg",
            (Guid)book.CategoryId!,
            (Guid)book.PublisherId!,
            [.. book.BookAuthors.Select(x => x.AuthorId)]
        );

        var repository = new Mock<IBookRepository>();
        repository
            .Setup(x => x.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        repository
            .Setup(x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new UpdateBookCommand(
            book.Id,
            book.Name!,
            book.Description!,
            null,
            book.Price!.OriginalPrice,
            book.Price.DiscountPrice,
            (Guid)book.CategoryId!,
            (Guid)book.PublisherId!,
            [.. book.BookAuthors.Select(x => x.AuthorId)]
        )
        {
            ImageName = "new-image.jpg",
        }; // Simulating a new image upload

        var handler = new UpdateBookHandler(repository.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        book.Image.ShouldBe("new-image.jpg");
    }

    [Test]
    public async Task GivenIsRemoveImageFalseAndNoNewImage_WhenHandling_ThenShouldKeepExistingImage()
    {
        // Arrange
        var bookFaker = new BookFaker();
        var book = bookFaker.Generate(1)[0];
        book.Update(
            book.Name!,
            book.Description,
            book.Price!.OriginalPrice,
            book.Price.DiscountPrice,
            "existing-image.jpg",
            (Guid)book.CategoryId!,
            (Guid)book.PublisherId!,
            [.. book.BookAuthors.Select(x => x.AuthorId)]
        );

        var repository = new Mock<IBookRepository>();
        repository
            .Setup(x => x.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        repository
            .Setup(x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new UpdateBookCommand(
            book.Id,
            book.Name!,
            book.Description!,
            null,
            book.Price!.OriginalPrice,
            book.Price.DiscountPrice,
            (Guid)book.CategoryId,
            (Guid)book.PublisherId,
            [.. book.BookAuthors.Select(x => x.AuthorId)]
        );

        var handler = new UpdateBookHandler(repository.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        book.Image.ShouldBe("existing-image.jpg");
    }

    #endregion

    #region PostUpdateBookHandler Tests

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
            ImageName = imageName,
        };

        var handler = new PostUpdateBookHandler(blobService.Object);

        // Act
        await handler.Process(command, Unit.Value, CancellationToken.None);

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
            ImageName = imageName,
        };

        var handler = new PostUpdateBookHandler(blobService.Object);

        // Act
        await handler.Process(command, Unit.Value, CancellationToken.None);

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

        var handler = new PostUpdateBookHandler(blobService.Object);

        // Act
        await handler.Process(command, Unit.Value, CancellationToken.None);

        // Assert
        blobService.Verify(
            x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion
}
