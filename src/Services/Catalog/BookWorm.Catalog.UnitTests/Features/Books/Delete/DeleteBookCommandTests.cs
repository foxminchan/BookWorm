using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Features.Books.Delete;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.SharedKernel.Exceptions;
using MediatR;

namespace BookWorm.Catalog.UnitTests.Features.Books.Delete;

public sealed class DeleteBookCommandTests
{
    private readonly BookFaker _faker;
    private readonly DeleteBookHandler _handler;
    private readonly Mock<IBookRepository> _repositoryMock;

    public DeleteBookCommandTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
        _faker = new();
    }

    [Test]
    public async Task GivenExistingBookId_WhenHandlingDeleteBookCommand_ThenShouldDeleteBookAndSaveChanges()
    {
        // Arrange
        var book = _faker.Generate(1).First();
        var command = new DeleteBookCommand(book.Id);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _repositoryMock
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _repositoryMock.Verify(
            r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public void GivenNonExistingBookId_WhenHandlingDeleteBookCommand_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var command = new DeleteBookCommand(bookId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book)null!);

        // Act & Assert
        var exception = Should.Throw<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Message.ShouldContain(bookId.ToString());
        _repositoryMock.Verify(
            r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
