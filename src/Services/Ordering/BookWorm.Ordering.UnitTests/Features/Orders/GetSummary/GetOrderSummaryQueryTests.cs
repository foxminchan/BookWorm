using System.Security.Claims;
using BookWorm.Chassis.Exceptions;
using BookWorm.Constants.Core;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Features.Orders.GetSummary;
using BookWorm.Ordering.Infrastructure.EventStore.Projections;
using JasperFx.Events;
using Marten;
using IQueryEventStore = Marten.Events.IQueryEventStore;

namespace BookWorm.Ordering.UnitTests.Features.Orders.GetSummary;

public sealed class GetOrderSummaryQueryTests
{
    private readonly Mock<IQueryEventStore> _eventsMock = new();
    private readonly OrderId _orderId = OrderId.From(Guid.CreateVersion7());
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<IQuerySession> _querySessionMock = new();

    public GetOrderSummaryQueryTests()
    {
        _querySessionMock.SetupGet(s => s.Events).Returns(_eventsMock.Object);
    }

    [Test]
    public async Task GivenExistingOrderStream_WhenGettingOrderSummary_ThenShouldReturnSummaryWithStreamVersion()
    {
        // Arrange
        var handler = CreateHandler(CreateAdminPrincipal());
        var expectedSummary = new OrderSummary((Guid)_orderId, Status.Completed, 125.50m);
        var streamState = new StreamState { Id = (Guid)_orderId, Version = 2 };

        _eventsMock
            .Setup(e =>
                e.AggregateStreamAsync<OrderSummary>(
                    (Guid)_orderId,
                    0,
                    null,
                    null,
                    0,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedSummary);
        _eventsMock
            .Setup(e => e.FetchStreamStateAsync((Guid)_orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(streamState);

        // Act
        var result = await handler.Handle(new(_orderId), CancellationToken.None);

        // Assert
        result.Id.ShouldBe((Guid)_orderId);
        result.Status.ShouldBe(Status.Completed);
        result.TotalPrice.ShouldBe(125.50m);
        result.Version.ShouldBe(2);
    }

    [Test]
    public async Task GivenSpecificVersion_WhenGettingOrderSummary_ThenShouldReplayUpToRequestedVersion()
    {
        // Arrange
        var handler = CreateHandler(CreateAdminPrincipal());
        var expectedSummary = new OrderSummary((Guid)_orderId, Status.New, 125.50m);

        _eventsMock
            .Setup(e =>
                e.AggregateStreamAsync<OrderSummary>(
                    (Guid)_orderId,
                    1,
                    null,
                    null,
                    0,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedSummary);
        _eventsMock
            .Setup(e => e.FetchStreamStateAsync((Guid)_orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StreamState?)null);

        // Act
        var result = await handler.Handle(new(_orderId, 1), CancellationToken.None);

        // Assert
        result.Status.ShouldBe(Status.New);
        result.Version.ShouldBe(1);
    }

    [Test]
    public async Task GivenTimestamp_WhenGettingOrderSummary_ThenShouldReplayAtRequestedTimestamp()
    {
        // Arrange
        var handler = CreateHandler(CreateAdminPrincipal());
        var at = DateTimeOffset.UtcNow.AddMinutes(-5);
        var expectedSummary = new OrderSummary((Guid)_orderId, Status.New, 125.50m);
        var streamState = new StreamState { Id = (Guid)_orderId, Version = 2 };

        _eventsMock
            .Setup(e =>
                e.AggregateStreamAsync<OrderSummary>(
                    (Guid)_orderId,
                    0,
                    at,
                    null,
                    0,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedSummary);
        _eventsMock
            .Setup(e => e.FetchStreamStateAsync((Guid)_orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(streamState);

        // Act
        var result = await handler.Handle(new(_orderId, At: at), CancellationToken.None);

        // Assert
        result.Status.ShouldBe(Status.New);
        result.Version.ShouldBe(2);
    }

    [Test]
    public async Task GivenMissingStream_WhenGettingOrderSummary_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var handler = CreateHandler(CreateAdminPrincipal());
        _eventsMock
            .Setup(e =>
                e.AggregateStreamAsync<OrderSummary>(
                    (Guid)_orderId,
                    0,
                    null,
                    null,
                    0,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((OrderSummary?)null);

        // Act
        Func<Task> act = async () => await handler.Handle(new(_orderId), CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    private GetOrderSummaryHandler CreateHandler(ClaimsPrincipal principal)
    {
        return new(_querySessionMock.Object, _orderRepositoryMock.Object, principal);
    }

    private static ClaimsPrincipal CreateAdminPrincipal()
    {
        return new(new ClaimsIdentity([new(ClaimTypes.Role, Authorization.Roles.Admin)], "Test"));
    }
}
