using BookWorm.Ordering.Features.Orders.Delete;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace BookWorm.Ordering.UnitTests.Features.Orders.Delete;

public sealed class DeleteOrderEndpointTests
{
    private readonly DeleteOrderEndpoint _endpoint = new();
    private readonly Guid _orderId = Guid.CreateVersion7();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidOrderId_WhenHandleAsync_ThenReturnsNoContent()
    {
        // Arrange
        _senderMock
            .Setup(s =>
                s.Send(
                    It.Is<DeleteOrderCommand>(c => c.Id == _orderId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(_orderId, _senderMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(StatusCodes.Status204NoContent);

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteOrderCommand>(c => c.Id == _orderId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
