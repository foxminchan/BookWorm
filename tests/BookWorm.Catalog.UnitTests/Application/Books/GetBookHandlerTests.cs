using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Catalog.Domain.BookAggregate.Specifications;
using BookWorm.Catalog.Features.Books.Get;
using BookWorm.Catalog.UnitTests.Builder;

namespace BookWorm.Catalog.UnitTests.Application.Books;

public sealed class GetBookHandlerTests
{
    private readonly Mock<IReadRepository<Book>> _bookRepositoryMock;
    private readonly GetBookHandler _handler;

    public GetBookHandlerTests()
    {
        _bookRepositoryMock = new();
        _handler = new(_bookRepositoryMock.Object);
    }

    [Fact]
    public async Task GivenRequest_ShouldReturnResult_WhenBookExists()
    {
        // Arrange
        var book = BookBuilder.WithDefaultValues()[0];
        var id = book.Id;

        _bookRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<BookFilterSpec>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var query = new GetBookQuery(id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(book);
        result.Value.Id.Should().Be(book.Id);
        result.Value.Name.Should().Be(book.Name);
        result.Value.Description.Should().Be(book.Description);
        _bookRepositoryMock.Verify(
            x => x.FirstOrDefaultAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GivenRequest_ShouldReturnNull_WhenBookDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _bookRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<BookFilterSpec>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var query = new GetBookQuery(id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
        _bookRepositoryMock.Verify(
            x => x.FirstOrDefaultAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
