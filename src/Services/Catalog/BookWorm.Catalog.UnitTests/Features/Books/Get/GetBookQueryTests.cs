using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Features.Books;
using BookWorm.Catalog.Features.Books.Get;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.SharedKernel.Exceptions;
using BookWorm.SharedKernel.Mapper;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Catalog.UnitTests.Features.Books.Get;

public sealed class GetBookQueryTests
{
    private readonly GetBookHandler _handler;
    private readonly Mock<IMapper<Book, BookDto>> _mapperMock;
    private readonly Mock<IBookRepository> _repositoryMock;

    public GetBookQueryTests()
    {
        _repositoryMock = new();
        _mapperMock = new();
        _handler = new(_repositoryMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task GivenValidId_WhenGetBookQueryHandled_ThenShouldReturnBookDto()
    {
        // Arrange
        var bookFaker = new BookFaker();
        var book = bookFaker.Generate(1)[0];
        var expectedBookDto = new BookDto(
            book.Id,
            book.Name,
            book.Description,
            book.Image,
            book.Price!.OriginalPrice,
            book.Price.DiscountPrice,
            book.Status,
            null, // Category
            null, // Publisher
            [], // Authors
            0, // AverageRating
            0 // TotalReviews
        );

        _repositoryMock
            .Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _mapperMock.Setup(m => m.MapToDto(book)).Returns(expectedBookDto);

        // Act
        var result = await _handler.Handle(new(book.Id), CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedBookDto);
        _repositoryMock.Verify(
            r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _mapperMock.Verify(m => m.MapToDto(book), Times.Once);
    }

    [Test]
    public async Task GivenNonExistentId_WhenGetBookQueryHandled_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.CreateVersion7();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book)null!);

        // Act
        var act = () => _handler.Handle(new(nonExistentId), CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Book with id {nonExistentId} not found.");
        _repositoryMock.Verify(
            r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _mapperMock.Verify(m => m.MapToDto(It.IsAny<Book>()), Times.Never);
    }
}
