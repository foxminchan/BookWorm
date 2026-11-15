using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Features.Orders;
using BookWorm.Ordering.Features.Orders.GetSummary;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.UnitTests.Features.Orders.GetSummary;

public sealed class GetOrderSummaryEndpointTests
{
    private readonly GetOrderSummaryEndpoint _endpoint = new();
    private readonly OrderId _orderId = OrderId.From(Guid.CreateVersion7());
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidOrderId_WhenHandlingGetOrderSummaryRequest_ThenShouldReturnSummary()
    {
        // Arrange
        var summary = new OrderSummaryDto((Guid)_orderId, Status.Completed, 120.00m, 2);

        _senderMock
            .Setup(x =>
                x.Send(
                    It.Is<GetOrderSummaryQuery>(q => q.Id == _orderId && q.Version == null),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(summary);

        // Act
        var result = await _endpoint.HandleAsync(new(_orderId), _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<OrderSummaryDto>>();
        result.Value.ShouldBe(summary);
    }
}
