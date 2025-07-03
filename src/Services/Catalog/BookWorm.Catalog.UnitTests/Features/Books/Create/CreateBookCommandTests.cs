using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Features.Books.Create;

namespace BookWorm.Catalog.UnitTests.Features.Books.Create;

public sealed class CreateBookCommandTests
{
    [Test]
    public async Task GivenValidCommand_WhenHandling_ThenShouldCreateBookCorrectly()
    {
        // Arrange
        var command = new CreateBookCommandFaker().Generate();
        command.ImageName = "test-image.jpg"; // Set image name as if pre-processor ran

        var bookId = Guid.CreateVersion7();

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
}
