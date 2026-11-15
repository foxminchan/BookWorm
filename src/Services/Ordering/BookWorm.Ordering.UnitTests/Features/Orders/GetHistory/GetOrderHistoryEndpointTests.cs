using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Features.Orders;
using BookWorm.Ordering.Features.Orders.GetHistory;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.UnitTests.Features.Orders.GetHistory;

public sealed class GetOrderHistoryEndpointTests
{
    private readonly GetOrderHistoryEndpoint _endpoint = new();
    private readonly OrderId _orderId = OrderId.From(Guid.CreateVersion7());
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidOrderId_WhenHandlingGetOrderHistoryRequest_ThenShouldReturnEventHistory()
    {
        // Arrange
        IReadOnlyList<OrderEventHistoryDto> history =
        [
            new(1, 1, "order_placed", DateTimeOffset.UtcNow, Status.New, 99.99m),
            new(2, 2, "order_completed", DateTimeOffset.UtcNow, Status.Completed, 99.99m),
        ];

        _senderMock
            .Setup(x =>
                x.Send(
                    It.Is<GetOrderHistoryQuery>(q => q.Id == _orderId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(history);

        // Act
        var result = await _endpoint.HandleAsync(_orderId, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<IReadOnlyList<OrderEventHistoryDto>>>();
        result.Value.ShouldBe(history);
    }
}
