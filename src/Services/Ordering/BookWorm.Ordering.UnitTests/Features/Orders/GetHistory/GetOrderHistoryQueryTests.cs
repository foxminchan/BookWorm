using System.Security.Claims;
using BookWorm.Chassis.Exceptions;
using BookWorm.Constants.Core;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Domain.Events;
using BookWorm.Ordering.Features.Orders.GetHistory;
using BookWorm.Ordering.UnitTests.Fakers;
using JasperFx.Events;
using Marten;
using IQueryEventStore = Marten.Events.IQueryEventStore;

namespace BookWorm.Ordering.UnitTests.Features.Orders.GetHistory;

public sealed class GetOrderHistoryQueryTests
{
    private readonly Mock<IQueryEventStore> _eventsMock = new();
    private readonly OrderId _orderId = OrderId.From(Guid.CreateVersion7());
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<IQuerySession> _querySessionMock = new();

    public GetOrderHistoryQueryTests()
    {
        _querySessionMock.SetupGet(s => s.Events).Returns(_eventsMock.Object);
    }

    [Test]
    public async Task GivenAdminRoleAndExistingStream_WhenGettingOrderHistory_ThenShouldReturnChronologicalEvents()
    {
        // Arrange
        var handler = CreateHandler(CreateAdminPrincipal());
        var order = new OrderFaker().Generate()[0];
        var placedEvent = CreateMockEvent(1, 1, "order_placed", new OrderPlacedEvent(order));
        var completedEvent = CreateMockEvent(
            2,
            2,
            "order_completed",
            new OrderCompletedEvent(order)
        );

        _eventsMock
            .Setup(e =>
                e.FetchStreamAsync((Guid)_orderId, 0, null, 0, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([placedEvent, completedEvent]);

        // Act
        var result = await handler.Handle(new(_orderId), CancellationToken.None);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Version.ShouldBe(1);
        result[0].Status.ShouldBe(Status.New);
        result[1].Version.ShouldBe(2);
        result[1].Status.ShouldBe(Status.Completed);
        result[1].TotalPrice.ShouldBe(order.TotalPrice);
    }

    [Test]
    public async Task GivenEmptyStream_WhenGettingOrderHistory_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var handler = CreateHandler(CreateAdminPrincipal());
        _eventsMock
            .Setup(e =>
                e.FetchStreamAsync((Guid)_orderId, 0, null, 0, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);

        // Act
        Func<Task> act = async () => await handler.Handle(new(_orderId), CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task GivenCancelledAndUnknownEvents_WhenGettingOrderHistory_ThenShouldMapCancelledAndNullForUnknown()
    {
        // Arrange
        var handler = CreateHandler(CreateAdminPrincipal());
        var order = new OrderFaker().Generate()[0];
        var cancelledEvent = CreateMockEvent(
            1,
            1,
            "order_cancelled",
            new OrderCancelledEvent(order)
        );
        var unknownEvent = CreateMockEvent(2, 2, "unknown_event", new());

        _eventsMock
            .Setup(e =>
                e.FetchStreamAsync((Guid)_orderId, 0, null, 0, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([cancelledEvent, unknownEvent]);

        // Act
        var result = await handler.Handle(new(_orderId), CancellationToken.None);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Status.ShouldBe(Status.Cancelled);
        result[0].TotalPrice.ShouldBe(order.TotalPrice);
        result[1].Status.ShouldBeNull();
        result[1].TotalPrice.ShouldBeNull();
    }

    [Test]
    public async Task GivenBuyerRoleAndOwnedOrder_WhenGettingOrderHistory_ThenShouldReturnChronologicalEvents()
    {
        // Arrange
        var buyerId = Guid.CreateVersion7();
        var handler = CreateHandler(CreateBuyerPrincipal(buyerId));
        var order = new OrderFaker().Generate()[0];
        var placedEvent = CreateMockEvent(1, 1, "order_placed", new OrderPlacedEvent(order));

        _orderRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _eventsMock
            .Setup(e =>
                e.FetchStreamAsync((Guid)_orderId, 0, null, 0, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([placedEvent]);

        // Act
        var result = await handler.Handle(new(_orderId), CancellationToken.None);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Version.ShouldBe(1);
        result[0].Status.ShouldBe(Status.New);
        result[0].TotalPrice.ShouldBe(order.TotalPrice);
    }

    private GetOrderHistoryHandler CreateHandler(ClaimsPrincipal _)
    {
        return new(_querySessionMock.Object);
    }

    private static ClaimsPrincipal CreateAdminPrincipal()
    {
        return new(new ClaimsIdentity([new(ClaimTypes.Role, Authorization.Roles.Admin)], "Test"));
    }

    private static ClaimsPrincipal CreateBuyerPrincipal(Guid buyerId)
    {
        return new(
            new ClaimsIdentity([new(ClaimTypes.NameIdentifier, buyerId.ToString())], "Test")
        );
    }

    private static IEvent CreateMockEvent(
        long sequence,
        long version,
        string eventTypeName,
        object data
    )
    {
        var mock = new Mock<IEvent>();
        mock.SetupGet(e => e.Sequence).Returns(sequence);
        mock.SetupGet(e => e.Version).Returns(version);
        mock.SetupGet(e => e.EventTypeName).Returns(eventTypeName);
        mock.SetupGet(e => e.Timestamp).Returns(DateTimeOffset.UtcNow);
        mock.SetupGet(e => e.Data).Returns(data);
        return mock.Object;
    }
}
