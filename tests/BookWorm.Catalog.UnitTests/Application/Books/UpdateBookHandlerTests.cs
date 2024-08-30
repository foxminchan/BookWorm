using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Catalog.Features.Books.Update;
using BookWorm.Catalog.UnitTests.Builder;

namespace BookWorm.Catalog.UnitTests.Application.Books;

public sealed class UpdateBookHandlerTests
{
    private readonly Mock<IAiService> _aiServiceMock;
    private readonly UpdateBookHandler _handler;
    private readonly Mock<IRepository<Book>> _repositoryMock;

    public UpdateBookHandlerTests()
    {
        _repositoryMock = new();
        _aiServiceMock = new();
        _handler = new(_repositoryMock.Object, _aiServiceMock.Object);
    }

    [Fact]
    public async Task GivenValidUpdateBookCommand_ShouldReturnSuccess()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = BookBuilder.WithDefaultValues()[0];
        var vector = new Vector(new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f });

        _repositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _repositoryMock.Setup(r => r.UpdateAsync(book, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _aiServiceMock
            .Setup(a => a.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vector);

        var command = new UpdateBookCommand(bookId, "Updated Name", "Updated Description", 120m, 90m, Status.OutOfStock,
            Guid.NewGuid(), Guid.NewGuid(),
            [Guid.NewGuid()]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        book.Name.Should().Be("Updated Name");
        book.Description.Should().Be("Updated Description");
        book.Price?.OriginalPrice.Should().Be(120m);
        book.Price?.DiscountPrice.Should().Be(90m);
        book.Status.Should().Be(Status.OutOfStock);
    }

    [Fact]
    public async Task GivenNonExistentBookId_ShouldThrowNotFoundException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var command = new UpdateBookCommand(bookId, "Updated Name", "Updated Description",
            120m, 90m,
            Status.OutOfStock, Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid()]);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _repositoryMock.Verify(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [CombinatorialData]
    public async Task GivenInvalidUpdateBookCommand_ShouldThrowValidationException(
        [CombinatorialValues(null, "", " ")] string name,
        [CombinatorialValues(null, "", " ")] string? description,
        [CombinatorialValues(-1, 0)] decimal price,
        [CombinatorialValues(-1, 0)] decimal priceSale,
        [CombinatorialValues((Status)99, (Status)100)]
        Status status)
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = BookBuilder.WithDefaultValues()[0];

        _repositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var command = new UpdateBookCommand(bookId, name, description, price, priceSale, status, Guid.NewGuid(),
            Guid.NewGuid(), [Guid.NewGuid()]);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
