using BookWorm.Ordering.Features.Buyers;
using BookWorm.Ordering.Features.Buyers.List;
using BookWorm.SharedKernel.Results;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.UnitTests.Features.Buyers.List;

public sealed class ListBuyersEndpointTests
{
    private readonly ListBuyersEndpoint _endpoint;
    private readonly PagedResult<BuyerDto> _pagedResult;
    private readonly Mock<ISender> _senderMock;
    private readonly ListBuyersQuery _validQuery;

    public ListBuyersEndpointTests()
    {
        // Setup common test data and mocks
        _senderMock = new();
        _endpoint = new();

        // Create test data
        _validQuery = new(1, 10);

        List<BuyerDto> buyers =
        [
            new(Guid.CreateVersion7(), "John Doe", "123 Main St"),
            new(Guid.CreateVersion7(), "Jane Smith", "456 Elm St"),
        ];

        _pagedResult = new(buyers, 1, 10, 2);
    }

    [Test]
    public async Task GivenValidQuery_WhenHandlingRequest_ThenShouldReturnPagedResult()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(_validQuery, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_pagedResult);

        // Act
        var result = await _endpoint.HandleAsync(_validQuery, _senderMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Ok<PagedResult<BuyerDto>>>();
        result.Value.ShouldBe(_pagedResult);
        _senderMock.Verify(s => s.Send(_validQuery, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandlingRequest_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        _senderMock
            .Setup(s => s.Send(_validQuery, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _endpoint.HandleAsync(_validQuery, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Test exception");
    }
}
