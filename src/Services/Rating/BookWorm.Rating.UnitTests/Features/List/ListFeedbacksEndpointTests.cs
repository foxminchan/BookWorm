using BookWorm.Rating.Features;
using BookWorm.Rating.Features.List;
using BookWorm.SharedKernel.SeedWork.Model;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Rating.UnitTests.Features.List;

public sealed class ListFeedbacksEndpointTests
{
    private readonly ListFeedbacksEndpoint _endpoint;
    private readonly PagedResult<FeedbackDto> _pagedResult;
    private readonly Mock<ISender> _senderMock;
    private readonly ListFeedbacksQuery _validQuery;

    public ListFeedbacksEndpointTests()
    {
        _senderMock = new();
        _endpoint = new();
        var bookId = Guid.CreateVersion7();

        // Create a valid query
        _validQuery = new(bookId, 1, 10);

        // Create sample feedback DTOs
        var feedbacks = new List<FeedbackDto>
        {
            new(Guid.CreateVersion7(), "John", "Doe", "Great book!", 5, bookId),
            new(Guid.CreateVersion7(), "Jane", "Smith", "Loved it", 4, bookId),
        };

        // Create paged result
        _pagedResult = new(feedbacks, 1, 10, 2, 1);
    }

    [Test]
    public async Task GivenValidQuery_WhenHandleAsync_ThenShouldReturnOkWithPagedResult()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(_validQuery, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_pagedResult);

        // Act
        var result = await _endpoint.HandleAsync(_validQuery, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<PagedResult<FeedbackDto>>>();
        result.Value.ShouldBe(_pagedResult);
        result.StatusCode.ShouldBe(StatusCodes.Status200OK);
        _senderMock.Verify(x => x.Send(_validQuery, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenValidQuery_WhenHandleAsyncWithCancellationToken_ThenShouldPassTokenToSender()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        _senderMock.Setup(x => x.Send(_validQuery, cancellationToken)).ReturnsAsync(_pagedResult);

        // Act
        var result = await _endpoint.HandleAsync(
            _validQuery,
            _senderMock.Object,
            cancellationToken
        );

        // Assert
        result.ShouldBeOfType<Ok<PagedResult<FeedbackDto>>>();
        _senderMock.Verify(x => x.Send(_validQuery, cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenEmptyResult_WhenHandleAsync_ThenShouldReturnOkWithEmptyPagedResult()
    {
        // Arrange
        var emptyResult = new PagedResult<FeedbackDto>(new List<FeedbackDto>(), 1, 10, 0, 0);

        _senderMock
            .Setup(x => x.Send(_validQuery, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);

        // Act
        var result = await _endpoint.HandleAsync(_validQuery, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<PagedResult<FeedbackDto>>>();
        result.Value.ShouldBe(emptyResult);
        result.Value?.Items.Count.ShouldBe(0);
        result.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }
}
