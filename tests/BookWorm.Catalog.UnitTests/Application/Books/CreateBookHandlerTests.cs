using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Catalog.Features.Books.Create;

namespace BookWorm.Catalog.UnitTests.Application.Books;

public sealed class CreateBookHandlerTests
{
    private readonly Mock<IAiService> _aiServiceMock;
    private readonly Mock<IAzuriteService> _azuriteMock;
    private readonly CreateBookHandler _handler;
    private readonly Mock<IRepository<Book>> _repositoryMock;

    public CreateBookHandlerTests()
    {
        _repositoryMock = new();
        _azuriteMock = new();
        _aiServiceMock = new();
        _handler = new(_repositoryMock.Object, _azuriteMock.Object, _aiServiceMock.Object);
    }

    [Fact]
    public async Task GivenValidCreateBookCommand_ShouldReturnBookId()
    {
        // Arrange
        var imageFileMock = new Mock<IFormFile>();
        var command = new CreateBookCommand(
            "Test Book",
            "Test Description",
            imageFileMock.Object,
            100m,
            80m,
            Status.InStock,
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );

        var vector = new Vector(new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f });
        var book = new Book(
            command.Name,
            command.Description,
            "http://example.com/image.jpg",
            command.Price,
            command.PriceSale,
            command.Status,
            command.CategoryId,
            command.PublisherId,
            command.AuthorIds
        );

        _azuriteMock
            .Setup(a => a.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://example.com/image.jpg");

        _aiServiceMock
            .Setup(a => a.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vector);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Should().Be(book.Id);
        _azuriteMock.Verify(
            a => a.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _aiServiceMock.Verify(
            a => a.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GivenNoImageFile_ShouldReturnBookIdWithoutUploadingFile()
    {
        // Arrange
        var command = new CreateBookCommand(
            "Test Book",
            "Test Description",
            null,
            100m,
            80m,
            Status.InStock,
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );

        var vector = new Vector(new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f });
        var book = new Book(
            command.Name,
            command.Description,
            null,
            command.Price,
            command.PriceSale,
            command.Status,
            command.CategoryId,
            command.PublisherId,
            command.AuthorIds
        );

        _aiServiceMock
            .Setup(a => a.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vector);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Should().Be(book.Id);
        _azuriteMock.Verify(
            a => a.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _aiServiceMock.Verify(
            a => a.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GivenAiServiceThrowsException_ShouldThrowException()
    {
        // Arrange
        var command = new CreateBookCommand(
            "Test Book",
            "Test Description",
            null,
            100m,
            80m,
            Status.InStock,
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );

        _aiServiceMock
            .Setup(a => a.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("AI Service failure"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("AI Service failure");
        _azuriteMock.Verify(
            a => a.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _aiServiceMock.Verify(
            a => a.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Theory]
    [CombinatorialData]
    public void GivenInvalidCreateBookCommand_ShouldThrowArgumentException_WhenParameterIsInvalid(
        [CombinatorialValues(null, "", " ")] string name,
        [CombinatorialValues(null, "", " ")] string? description,
        [CombinatorialValues(-1, 0)] decimal price,
        [CombinatorialValues(-1, 0)] decimal priceSale,
        [CombinatorialValues((Status)99, (Status)100)] Status status
    )
    {
        // Arrange
        var command = new CreateBookCommand(
            name,
            description,
            null,
            price,
            priceSale,
            status,
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }
}
