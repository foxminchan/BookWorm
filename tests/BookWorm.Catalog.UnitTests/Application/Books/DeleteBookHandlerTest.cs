using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Catalog.Features.Books.Delete;
using BookWorm.Catalog.UnitTests.Builder;

namespace BookWorm.Catalog.UnitTests.Application.Books;

public sealed class DeleteBookHandlerTest
{
    private readonly Mock<IRepository<Book>> _bookRepositoryMock;
    private readonly DeleteBookHandler _handler;

    public DeleteBookHandlerTest()
    {
        _bookRepositoryMock = new();
        _handler = new(_bookRepositoryMock.Object);
    }

    [Fact]
    public async Task GivenRequest_ShouldDeleteBook_WhenBookExists()
    {
        // Arrange
        var book = BookBuilder.WithDefaultValues()[0];
        var id = book.Id;
        var command = new DeleteBookCommand(id);

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _bookRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _bookRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GivenRequest_ShouldReturnNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var command = new DeleteBookCommand(id);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _bookRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _bookRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
