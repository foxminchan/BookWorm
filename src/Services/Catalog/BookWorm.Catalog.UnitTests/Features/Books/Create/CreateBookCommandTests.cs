using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Features.Books.Create;
using BookWorm.Catalog.Infrastructure.Blob;
using Microsoft.AspNetCore.Http;

namespace BookWorm.Catalog.UnitTests.Features.Books.Create;

public sealed class CreateBookCommandTests
{
    #region PreCreateBookHandler Tests

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
        var command = new CreateBookCommandFaker().Generate();
        command = command with { Image = mockFile.Object };

        var handler = new PreCreateBookHandler(mockBlobService.Object);

        // Act
        await handler.Process(command, CancellationToken.None);

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
        var command = new CreateBookCommandFaker().Generate();
        command = command with { Image = null };

        var handler = new PreCreateBookHandler(mockBlobService.Object);

        // Act
        await handler.Process(command, CancellationToken.None);

        // Assert
        command.ImageName.ShouldBeNull();
        mockBlobService.Verify(
            s => s.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion


    #region CreateBookHandler Tests

    [Test]
    public async Task GivenValidCommand_WhenHandling_ThenShouldCreateBookCorrectly()
    {
        // Arrange
        var command = new CreateBookCommandFaker().Generate();
        command.ImageName = "test-image.jpg"; // Set image name as if pre-processor ran

        var bookId = Guid.NewGuid();

        var mockRepo = new Mock<IBookRepository>();
        mockRepo
            .Setup(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (Book b, CancellationToken _) =>
                {
                    typeof(Book).GetProperty("Id")?.SetValue(b, bookId);
                    return b;
                }
            );

        mockRepo
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateBookHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(bookId);
        mockRepo.Verify(
            r =>
                r.AddAsync(
                    It.Is<Book>(b =>
                        b.Name == command.Name
                        && b.Description == command.Description
                        && b.Image == command.ImageName
                        && b.Price!.OriginalPrice == command.Price
                        && b.Price!.DiscountPrice == command.PriceSale
                        && b.CategoryId == command.CategoryId
                        && b.PublisherId == command.PublisherId
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        mockRepo.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidCommand_WhenHandling_ThenShouldCreateBookWithCorrectAuthors()
    {
        // Arrange
        var command = new CreateBookCommandFaker().Generate();
        Book? capturedBook = null;

        var mockRepo = new Mock<IBookRepository>();
        mockRepo
            .Setup(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Callback<Book, CancellationToken>((book, _) => capturedBook = book)
            .ReturnsAsync((Book b, CancellationToken _) => b);

        mockRepo
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateBookHandler(mockRepo.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedBook.ShouldNotBeNull();
        capturedBook!.BookAuthors.Count.ShouldBe(command.AuthorIds.Length);

        foreach (var authorId in command.AuthorIds)
        {
            capturedBook!.BookAuthors.ShouldContain(ba => ba.AuthorId == authorId);
        }
    }

    #endregion
}
