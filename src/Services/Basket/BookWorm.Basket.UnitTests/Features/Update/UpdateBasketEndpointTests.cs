using BookWorm.Basket.Features;
using BookWorm.Basket.Features.Update;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Basket.UnitTests.Features.Update;

public sealed class UpdateBasketEndpointTests
{
    private readonly UpdateBasketEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidCommand_WhenHandlingRequest_ThenShouldCallSenderAndReturnNoContent()
    {
        // Arrange
        var command = new UpdateBasketCommand(
            [new BasketItemRequest("item1", 1), new BasketItemRequest("item2", 2)]
        );

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result = await _endpoint.HandleAsync(command, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(s => s.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandlingRequest_ThenShouldPropagateException()
    {
        // Arrange
        var command = new UpdateBasketCommand(new());
        var expectedException = new InvalidOperationException("Test exception");

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _endpoint.HandleAsync(command, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Test exception");
    }

    [Test]
    public async Task GivenEmptyItemsList_WhenHandlingRequest_ThenShouldCallSenderWithCorrectCommand()
    {
        // Arrange
        var items = new List<BasketItemRequest>();
        var command = new UpdateBasketCommand(items);

        _senderMock
            .Setup(s => s.Send(It.IsAny<UpdateBasketCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        await _endpoint.HandleAsync(command, _senderMock.Object);

        // Assert
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<UpdateBasketCommand>(c => c.Items == items),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
