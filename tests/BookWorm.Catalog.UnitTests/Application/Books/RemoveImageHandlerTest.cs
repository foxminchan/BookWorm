using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Catalog.Features.Books.RemoveImage;
using BookWorm.Catalog.UnitTests.Builder;

namespace BookWorm.Catalog.UnitTests.Application.Books;

public sealed class RemoveBookImageHandlerTests
{
    private readonly Mock<IAzuriteService> _azuriteMock;
    private readonly RemoveBookImageHandler _handler;
    private readonly Mock<IRepository<Book>> _repositoryMock;

    public RemoveBookImageHandlerTests()
    {
        _repositoryMock = new();
        _azuriteMock = new();
        _handler = new(_repositoryMock.Object, _azuriteMock.Object);
    }

    [Fact]
    public async Task GivenValidRemoveBookImageCommand_ShouldRemoveBookImageAndReturnSuccess_WhenBookExists()
    {
        // Arrange
        var book = BookBuilder.WithDefaultValues()[0];
        var bookId = book.Id;

        _repositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _azuriteMock.Setup(a => a.DeleteFileAsync(book.ImageUrl!, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock.Setup(r => r.UpdateAsync(book, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RemoveBookImageCommand(bookId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        book.ImageUrl.Should().BeNull();
    }

    [Fact]
    public async Task GivenNonExistentBookId_ShouldThrowNotFoundException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var command = new RemoveBookImageCommand(bookId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _repositoryMock.Verify(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
        _azuriteMock.Verify(a => a.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GivenBookWithNoImageUrl_WhenHandlingCommand_ThenShouldNotCallAzuriteDeleteFileAsync()
    {
        // Arrange
        var book = BookBuilder.WithDefaultValues()[1];
        var bookId = book.Id;

        _repositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var command = new RemoveBookImageCommand(bookId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        book.ImageUrl.Should().BeNull();
    }
}
