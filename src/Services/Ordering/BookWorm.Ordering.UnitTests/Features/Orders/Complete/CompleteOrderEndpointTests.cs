using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Features.Orders;
using BookWorm.Ordering.Features.Orders.Complete;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.UnitTests.Features.Orders.Complete;

public sealed class CompleteOrderEndpointTests
{
    private readonly CompleteOrderEndpoint _endpoint;
    private readonly OrderDetailDto _orderDetailDto;
    private readonly Guid _orderId;
    private readonly Mock<ISender> _senderMock;

    public CompleteOrderEndpointTests()
    {
        _endpoint = new();
        _senderMock = new();
        _orderId = Guid.CreateVersion7();

        // Create a sample OrderDetailDto for testing
        _orderDetailDto = new(
            _orderId,
            DateTime.UtcNow,
            99.99m,
            Status.Completed,
            new List<OrderItemDto> { new(Guid.CreateVersion7(), 2, 49.99m) { Name = "Test Book" } }
        );
    }

    [Test]
    public async Task GivenValidOrderId_WhenHandlingCompleteOrder_ThenShouldReturnOrderDetails()
    {
        // Arrange
        _senderMock
            .Setup(x =>
                x.Send(
                    It.Is<CompleteOrderCommand>(c => c.OrderId == _orderId),
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
                    It.Is<CompleteOrderCommand>(c => c.OrderId == _orderId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCancellationToken_WhenHandlingCompleteOrder_ThenShouldPassTokenToSender()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        _senderMock
            .Setup(x => x.Send(It.IsAny<CompleteOrderCommand>(), cancellationToken))
            .ReturnsAsync(_orderDetailDto);

        // Act
        await _endpoint.HandleAsync(_orderId, _senderMock.Object, cancellationToken);

        // Assert
        _senderMock.Verify(
            x => x.Send(It.IsAny<CompleteOrderCommand>(), cancellationToken),
            Times.Once
        );
    }
}
