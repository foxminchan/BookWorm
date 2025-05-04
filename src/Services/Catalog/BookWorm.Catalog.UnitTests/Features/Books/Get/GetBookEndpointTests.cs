using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Features.Books;
using BookWorm.Catalog.Features.Books.Get;
using BookWorm.Chassis.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Books.Get;

public sealed class GetBookEndpointTests
{
    private readonly BookDto _bookDto;
    private readonly Guid _bookId;
    private readonly GetBookEndpoint _endpoint;
    private readonly Mock<ISender> _senderMock;

    public GetBookEndpointTests()
    {
        _senderMock = new();
        _endpoint = new();
        _bookId = Guid.CreateVersion7();

        // Create a sample BookDto with minimal required fields
        _bookDto = new(
            _bookId,
            "Test Book",
            "Test Description",
            "https://example.com/image.jpg",
            19.99m,
            null,
            Status.InStock,
            null,
            null,
            [],
            4.5,
            10
        );
    }

    [Test]
    public async Task GivenValidBookId_WhenHandlingGetBookRequest_ThenShouldReturnOkWithBookDto()
    {
        // Arrange
        _senderMock
            .Setup(x =>
                x.Send(It.Is<GetBookQuery>(q => q.Id == _bookId), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(_bookDto);

        // Act
        var result = await _endpoint.HandleAsync(_bookId, _senderMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Ok<BookDto>>();
        result.Value.ShouldBe(_bookDto);
        result.Value?.Id.ShouldBe(_bookId);
        result.Value?.Name.ShouldBe(_bookDto.Name);
        result.Value?.Description.ShouldBe(_bookDto.Description);
        result.Value?.ImageUrl.ShouldBe(_bookDto.ImageUrl);
        result.Value?.Price.ShouldBe(_bookDto.Price);
        result.Value?.PriceSale.ShouldBe(_bookDto.PriceSale);
        result.Value?.Status.ShouldBe(_bookDto.Status);
        result.Value?.Category.ShouldBe(_bookDto.Category);
        result.Value?.Publisher.ShouldBe(_bookDto.Publisher);
        result.Value?.Authors.Count.ShouldBe(_bookDto.Authors.Count);
        result.Value?.AverageRating.ShouldBe(_bookDto.AverageRating);
        result.Value?.TotalReviews.ShouldBe(_bookDto.TotalReviews);

        _senderMock.Verify(
            x => x.Send(It.Is<GetBookQuery>(q => q.Id == _bookId), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNonExistentBookId_WhenHandlingGetBookRequest_ThenShouldThrowNotFoundException()
    {
        // Arrange
        _senderMock
            .Setup(x =>
                x.Send(It.Is<GetBookQuery>(q => q.Id == _bookId), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new NotFoundException($"Book with id {_bookId} not found."));

        // Act
        var act = async () => await _endpoint.HandleAsync(_bookId, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Book with id {_bookId} not found.");

        _senderMock.Verify(
            x => x.Send(It.Is<GetBookQuery>(q => q.Id == _bookId), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
