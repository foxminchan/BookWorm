using BookWorm.Ordering.Features.Orders;
using BookWorm.Ordering.Features.Orders.Get;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.UnitTests.Features.Orders.Get;

public sealed class GetOrderEndpointTests
{
    private readonly GetOrderEndpoint _endpoint;
    private readonly OrderDetailDto _orderDetailDto;
    private readonly Guid _orderId;
    private readonly Mock<ISender> _senderMock;

    public GetOrderEndpointTests()
    {
        _senderMock = new();
        _endpoint = new();

        // Generate test data
        _orderId = Guid.CreateVersion7();

        var faker = new Faker();
        var orderItems = new List<OrderItemDto>
        {
            new(Guid.CreateVersion7(), faker.Random.Int(1, 10), faker.Random.Decimal(10, 100))
            {
                Name = faker.Commerce.ProductName(),
            },
        };

        _orderDetailDto = new(
            _orderId,
            faker.Date.Recent(),
            faker.Random.Decimal(50, 500),
            orderItems
        );
    }

    [Test]
    public async Task GivenValidOrderId_WhenHandlingGetOrderRequest_ThenShouldReturnOrderDetail()
    {
        // Arrange
        _senderMock
            .Setup(x =>
                x.Send(It.Is<GetOrderQuery>(q => q.Id == _orderId), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(_orderDetailDto);

        // Act
        var result = await _endpoint.HandleAsync(_orderId, _senderMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Ok<OrderDetailDto>>();
        result.Value.ShouldBe(_orderDetailDto);
        _senderMock.Verify(
            x => x.Send(It.Is<GetOrderQuery>(q => q.Id == _orderId), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSender_WhenHandlingGetOrderRequest_ThenShouldSendCorrectQuery()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(It.IsAny<GetOrderQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_orderDetailDto);

        // Act
        await _endpoint.HandleAsync(_orderId, _senderMock.Object);

        // Assert
        _senderMock.Verify(
            x => x.Send(It.Is<GetOrderQuery>(q => q.Id == _orderId), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
