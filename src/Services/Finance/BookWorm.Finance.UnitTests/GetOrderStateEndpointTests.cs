using BookWorm.Finance.Feature;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Finance.UnitTests;

public sealed class GetOrderStateEndpointTests
{
    private readonly GetOrderStateEndpoint _endpoint = new();
    private readonly Mock<ISender> _mockSender = new();

    [Test]
    public async Task GivenValidRequest_WhenHandlingGetOrderState_ThenShouldReturnOrderState()
    {
        // Arrange
        const string expectedResult = "Test Order State";
        _mockSender
            .Setup(s => s.Send(It.IsAny<GetOrderStateQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _endpoint.HandleAsync(_mockSender.Object);

        // Assert
        result.ShouldBeOfType<Ok<string>>();
        result.Value.ShouldBe(expectedResult);
        _mockSender.Verify(
            s => s.Send(It.IsAny<GetOrderStateQuery>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCancellationToken_WhenHandlingGetOrderState_ThenShouldPassTokenToQuery()
    {
        // Arrange
        var cancellationToken = new CancellationToken(true);

        // Act
        await _endpoint.HandleAsync(_mockSender.Object, cancellationToken);

        // Assert
        _mockSender.Verify(
            s => s.Send(It.IsAny<GetOrderStateQuery>(), cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task GivenExceptionDuringQuery_WhenHandlingGetOrderState_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        _mockSender
            .Setup(s => s.Send(It.IsAny<GetOrderStateQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _endpoint.HandleAsync(_mockSender.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Test exception");
    }
}
