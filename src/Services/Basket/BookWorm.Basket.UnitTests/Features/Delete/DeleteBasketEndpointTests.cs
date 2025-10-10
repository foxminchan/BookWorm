using BookWorm.Basket.Features.Delete;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Basket.UnitTests.Features.Delete;

public sealed class DeleteBasketEndpointTests
{
    private readonly DeleteBasketEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidRequest_WhenHandlingDeleteBasket_ThenShouldSendDeleteBasketCommand()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(It.IsAny<DeleteBasketCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(_senderMock.Object);

        // Assert
        _senderMock.Verify(
            x => x.Send(It.IsAny<DeleteBasketCommand>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        result.ShouldBeOfType<NoContent>();
    }

    [Test]
    public async Task GivenException_WhenHandlingDeleteBasket_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        _senderMock
            .Setup(x => x.Send(It.IsAny<DeleteBasketCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _endpoint.HandleAsync(_senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Test exception");
    }
}
