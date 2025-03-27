using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Features.Authors;
using BookWorm.Catalog.Features.Books;
using BookWorm.Catalog.Features.Books.List;
using BookWorm.SharedKernel.SeedWork.Model;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Books.List;

public sealed class ListBooksEndpointTests
{
    private readonly ListBooksEndpoint _endpoint;
    private readonly PagedResult<BookDto> _expectedResult;
    private readonly Mock<ISender> _senderMock;
    private readonly ListBooksQuery _validQuery;

    public ListBooksEndpointTests()
    {
        _senderMock = new();
        _endpoint = new();

        // Create a valid query with default values
        _validQuery = new(1, 10);

        // Create an expected result
        var books = new List<BookDto>
        {
            new(
                Guid.CreateVersion7(),
                "Book 1",
                "Description 1",
                "image1.jpg",
                19.99m,
                null,
                Status.InStock,
                null,
                null,
                new List<AuthorDto>(),
                4.5,
                10
            ),
        };

        _expectedResult = new(books, 1, 10, 1, 1);
    }

    [Test]
    public async Task GivenValidQuery_WhenHandlingRequest_ThenShouldReturnOkResultWithPagedBooks()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(_validQuery, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedResult);

        // Act
        var result = await _endpoint.HandleAsync(_validQuery, _senderMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Ok<PagedResult<BookDto>>>();
        result.Value.ShouldBe(_expectedResult);
        result.Value?.Items.Count.ShouldBe(1);
        result.Value?.PageIndex.ShouldBe(1);
        result.Value?.PageSize.ShouldBe(10);
        result.Value?.TotalItems.ShouldBe(1);
        result.Value?.TotalPages.ShouldBe(1);

        _senderMock.Verify(x => x.Send(_validQuery, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenEmptyResult_WhenHandlingRequest_ThenShouldReturnOkResultWithEmptyList()
    {
        // Arrange
        var emptyResult = new PagedResult<BookDto>(new List<BookDto>(), 1, 10, 0, 0);

        _senderMock
            .Setup(x => x.Send(_validQuery, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);

        // Act
        var result = await _endpoint.HandleAsync(_validQuery, _senderMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Ok<PagedResult<BookDto>>>();
        result.Value?.Items.Count.ShouldBe(0);
        result.Value?.TotalItems.ShouldBe(0);
        result.Value?.TotalPages.ShouldBe(0);
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandlingRequest_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");

        _senderMock
            .Setup(x => x.Send(_validQuery, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _endpoint.HandleAsync(_validQuery, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Test exception");

        _senderMock.Verify(x => x.Send(_validQuery, It.IsAny<CancellationToken>()), Times.Once);
    }
}
