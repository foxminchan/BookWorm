using BookWorm.Chassis.Exceptions;
using BookWorm.Ordering.Features.Buyers.Delete;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.UnitTests.Features.Buyers.Delete;

public sealed class DeleteBuyerEndpointTests
{
    private readonly Guid _buyerId = Guid.CreateVersion7();
    private readonly DeleteBuyerEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidBuyerId_WhenHandleAsyncCalled_ThenShouldSendDeleteCommandAndReturnNoContent()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(It.IsAny<DeleteBuyerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(_buyerId, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(
            x =>
                x.Send(
                    It.Is<DeleteBuyerCommand>(c => c.Id == _buyerId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNonExistentBuyerId_WhenHandleAsyncCalled_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var exceptionMessage = $"Buyer with ID {_buyerId} not found.";
        _senderMock
            .Setup(x => x.Send(It.IsAny<DeleteBuyerCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(exceptionMessage));

        // Act
        var act = async () => await _endpoint.HandleAsync(_buyerId, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe(exceptionMessage);
    }
}
