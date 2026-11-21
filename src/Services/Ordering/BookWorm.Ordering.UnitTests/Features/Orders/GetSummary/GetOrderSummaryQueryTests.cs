using System.Security.Claims;
using BookWorm.Chassis.Exceptions;
using BookWorm.Constants.Core;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Features.Orders.GetSummary;
using BookWorm.Ordering.Infrastructure.EventStore.Projections;
using BookWorm.Ordering.UnitTests.Fakers;
using JasperFx.Events;
using Marten;
using ZiggyCreatures.Caching.Fusion;
using IQueryEventStore = Marten.Events.IQueryEventStore;

namespace BookWorm.Ordering.UnitTests.Features.Orders.GetSummary;

public sealed class GetOrderSummaryQueryTests : IDisposable
{
    private readonly FusionCache _cache = new(new FusionCacheOptions());
    private readonly Mock<IQueryEventStore> _eventsMock = new();
    private readonly OrderId _orderId = OrderId.From(Guid.CreateVersion7());
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<IQuerySession> _querySessionMock = new();

    public GetOrderSummaryQueryTests()
    {
        _querySessionMock.SetupGet(s => s.Events).Returns(_eventsMock.Object);
    }

    public void Dispose()
    {
        _cache.Dispose();
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

    [Test]
    public async Task GivenNullStreamStateAndNoRequestedVersion_WhenGettingOrderSummary_ThenShouldReturnZeroVersion()
    {
        // Arrange
        var handler = CreateHandler(CreateAdminPrincipal());
        var expectedSummary = new OrderSummary((Guid)_orderId, Status.New, 125.50m);

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
            .ReturnsAsync((StreamState?)null);

        // Act
        var result = await handler.Handle(new(_orderId), CancellationToken.None);

        // Assert
        result.Version.ShouldBe(0);
    }

    [Test]
    public async Task GivenBuyerRoleAndOwnedOrder_WhenGettingOrderSummary_ThenShouldReturnSummary()
    {
        // Arrange
        var buyerId = Guid.CreateVersion7();
        var handler = CreateHandler(CreateBuyerPrincipal(buyerId));
        var order = new OrderFaker().Generate()[0];
        var expectedSummary = new OrderSummary((Guid)_orderId, Status.Completed, order.TotalPrice);
        var streamState = new StreamState { Id = (Guid)_orderId, Version = 2 };

        _orderRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
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
        result.TotalPrice.ShouldBe(order.TotalPrice);
        result.Version.ShouldBe(2);
    }

    private GetOrderSummaryHandler CreateHandler(ClaimsPrincipal _)
    {
        return new(_querySessionMock.Object, _cache);
    }

    [Test]
    public async Task GivenRequestedVersion_WhenGettingOrderSummaryTwice_ThenShouldReturnFromCache()
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

        // Act
        var first = await handler.Handle(new(_orderId, 1), CancellationToken.None);
        var second = await handler.Handle(new(_orderId, 1), CancellationToken.None);

        // Assert
        first.Version.ShouldBe(1);
        second.Version.ShouldBe(1);
        _eventsMock.Verify(
            e =>
                e.AggregateStreamAsync<OrderSummary>(
                    (Guid)_orderId,
                    1,
                    null,
                    null,
                    0,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    [Arguments(Status.Completed)]
    [Arguments(Status.Cancelled)]
    public async Task GivenTerminalOrder_WhenGettingOrderSummaryTwice_ThenShouldReturnFromCache(
        Status status
    )
    {
        // Arrange
        var handler = CreateHandler(CreateAdminPrincipal());
        var expectedSummary = new OrderSummary((Guid)_orderId, status, 99.00m);
        var streamState = new StreamState { Id = (Guid)_orderId, Version = 3 };

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
        var first = await handler.Handle(new(_orderId), CancellationToken.None);
        var second = await handler.Handle(new(_orderId), CancellationToken.None);

        // Assert
        first.Status.ShouldBe(status);
        second.Status.ShouldBe(status);
        _eventsMock.Verify(
            e =>
                e.AggregateStreamAsync<OrderSummary>(
                    (Guid)_orderId,
                    0,
                    null,
                    null,
                    0,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNonTerminalOrder_WhenGettingOrderSummaryTwice_ThenShouldNotCache()
    {
        // Arrange
        var handler = CreateHandler(CreateAdminPrincipal());
        var expectedSummary = new OrderSummary((Guid)_orderId, Status.New, 99.00m);
        var streamState = new StreamState { Id = (Guid)_orderId, Version = 1 };

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
        await handler.Handle(new(_orderId), CancellationToken.None);
        await handler.Handle(new(_orderId), CancellationToken.None);

        // Assert
        _eventsMock.Verify(
            e =>
                e.AggregateStreamAsync<OrderSummary>(
                    (Guid)_orderId,
                    0,
                    null,
                    null,
                    0,
                    It.IsAny<CancellationToken>()
                ),
            Times.Exactly(2)
        );
    }

    [Test]
    public async Task GivenTimestamp_WhenGettingOrderSummaryTwice_ThenShouldNotCache()
    {
        // Arrange
        var handler = CreateHandler(CreateAdminPrincipal());
        var at = DateTimeOffset.UtcNow.AddMinutes(-5);
        var expectedSummary = new OrderSummary((Guid)_orderId, Status.Completed, 99.00m);
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
        await handler.Handle(new(_orderId, At: at), CancellationToken.None);
        await handler.Handle(new(_orderId, At: at), CancellationToken.None);

        // Assert
        _eventsMock.Verify(
            e =>
                e.AggregateStreamAsync<OrderSummary>(
                    (Guid)_orderId,
                    0,
                    at,
                    null,
                    0,
                    It.IsAny<CancellationToken>()
                ),
            Times.Exactly(2)
        );
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
}
