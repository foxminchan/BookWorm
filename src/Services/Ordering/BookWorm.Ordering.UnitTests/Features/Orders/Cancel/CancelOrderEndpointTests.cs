using BookWorm.Ordering.Features.Orders;
using BookWorm.Ordering.Features.Orders.Cancel;
using BookWorm.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.UnitTests.Features.Orders.Cancel;

public sealed class CancelOrderEndpointTests
{
    private readonly CancelOrderEndpoint _endpoint;
    private readonly OrderDetailDto _orderDetailDto;
    private readonly Guid _orderId;
    private readonly Mock<ISender> _senderMock;

    public CancelOrderEndpointTests()
    {
        _senderMock = new();
        _endpoint = new();
        _orderId = Guid.CreateVersion7();

        // Create a sample OrderDetailDto to return from the sender
        _orderDetailDto = new(_orderId, DateTime.UtcNow, 100.0m, new List<OrderItemDto>());
    }

    [Test]
    public async Task GivenValidOrderId_WhenHandleAsync_ThenShouldReturnOrderDetailDto()
    {
        // Arrange
        _senderMock
            .Setup(x =>
                x.Send(
                    It.Is<CancelOrderCommand>(c => c.OrderId == _orderId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(_orderDetailDto);

        // Act
        var result = await _endpoint.HandleAsync(_orderId, _senderMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Ok<OrderDetailDto>>();
        result.Value.ShouldBe(_orderDetailDto);
        _senderMock.Verify(
            x =>
                x.Send(
                    It.Is<CancelOrderCommand>(c => c.OrderId == _orderId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSenderThrowsNotFoundException_WhenHandleAsync_ThenShouldPropagateException()
    {
        // Arrange
        _senderMock
            .Setup(x =>
                x.Send(
                    It.Is<CancelOrderCommand>(c => c.OrderId == _orderId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new NotFoundException($"Order with id {_orderId} not found"));

        // Act
        var act = async () => await _endpoint.HandleAsync(_orderId, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Order with id {_orderId} not found");
        _senderMock.Verify(
            x =>
                x.Send(
                    It.Is<CancelOrderCommand>(c => c.OrderId == _orderId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandleAsync_ThenShouldPropagateException()
    {
        // Arrange
        _senderMock
            .Setup(x =>
                x.Send(
                    It.Is<CancelOrderCommand>(c => c.OrderId == _orderId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new InvalidOperationException("Cannot cancel order in current state"));

        // Act
        var act = async () => await _endpoint.HandleAsync(_orderId, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Cannot cancel order in current state");
        _senderMock.Verify(
            x =>
                x.Send(
                    It.Is<CancelOrderCommand>(c => c.OrderId == _orderId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
