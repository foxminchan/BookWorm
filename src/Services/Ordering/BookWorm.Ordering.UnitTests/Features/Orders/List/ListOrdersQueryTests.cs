using System.Security.Claims;
using BookWorm.Constants.Core;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Features.Orders.List;
using BookWorm.Ordering.UnitTests.Fakers;

namespace BookWorm.Ordering.UnitTests.Features.Orders.List;

public sealed class ListOrdersQueryTests
{
    private readonly Mock<ClaimsPrincipal> _claimsPrincipalMock;
    private readonly Order[] _orders;
    private readonly Mock<IOrderRepository> _repositoryMock;

    public ListOrdersQueryTests()
    {
        var userId = Guid.CreateVersion7();
        _orders = new OrderFaker().Generate();

        _repositoryMock = new();
        _claimsPrincipalMock = new();

        // Setup default claims for a regular user
        var claim = new Claim(ClaimTypes.NameIdentifier, userId.ToString());
        _claimsPrincipalMock.Setup(x => x.FindFirst(ClaimTypes.NameIdentifier)).Returns(claim);
        _claimsPrincipalMock
            .Setup(x => x.FindAll(ClaimTypes.Role))
            .Returns([new(ClaimTypes.Role, Authorization.Roles.User)]);
    }

    [Test]
    public async Task GivenRegularUser_WhenListingOrders_ThenShouldFilterByUserId()
    {
        // Arrange
        var query = new ListOrdersQuery(1, 10);
        var handler = new ListOrdersHandler(_repositoryMock.Object, _claimsPrincipalMock.Object);

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_orders);
        _repositoryMock
            .Setup(x => x.CountAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_orders.Length);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Count.ShouldBe(_orders.Length);
        result.PageIndex.ShouldBe(1);
        result.PageSize.ShouldBe(10);
        result.TotalItems.ShouldBe(_orders.Length);

        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            x => x.CountAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenAdminUser_WhenListingOrders_ThenShouldNotReplaceBuyerId()
    {
        // Arrange
        var buyerId = Guid.CreateVersion7();
        var query = new ListOrdersQuery(1, 10, null, buyerId);

        // Setup admin role
        _claimsPrincipalMock
            .Setup(x => x.FindAll(ClaimTypes.Role))
            .Returns([new(ClaimTypes.Role, Authorization.Roles.Admin)]);

        var handler = new ListOrdersHandler(_repositoryMock.Object, _claimsPrincipalMock.Object);

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_orders);
        _repositoryMock
            .Setup(x => x.CountAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_orders.Length);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Count.ShouldBe(_orders.Length);
    }

    [Test]
    public async Task GivenStatusFilter_WhenListingOrders_ThenShouldApplyStatusFilter()
    {
        // Arrange
        const Status status = Status.Completed;
        var query = new ListOrdersQuery(1, 10, status);
        var handler = new ListOrdersHandler(_repositoryMock.Object, _claimsPrincipalMock.Object);

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_orders);
        _repositoryMock
            .Setup(x => x.CountAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_orders.Length);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Count.ShouldBe(_orders.Length);

        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            x => x.CountAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCustomPagination_WhenListingOrders_ThenShouldApplyPagination()
    {
        // Arrange
        const int pageIndex = 2;
        const int pageSize = 5;
        var query = new ListOrdersQuery(pageIndex, pageSize);
        var handler = new ListOrdersHandler(_repositoryMock.Object, _claimsPrincipalMock.Object);

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_orders);
        _repositoryMock
            .Setup(x => x.CountAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(15); // Total items for pagination calculation

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.PageIndex.ShouldBe(pageIndex);
        result.PageSize.ShouldBe(pageSize);
        result.TotalItems.ShouldBe(15);
        result.TotalPages.ShouldBe(3); // 15 items with page size 5 = 3 pages
        result.HasPreviousPage.ShouldBeTrue();
        result.HasNextPage.ShouldBeTrue();

        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            x => x.CountAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyOrdersList_WhenListingOrders_ThenShouldReturnEmptyResult()
    {
        // Arrange
        var query = new ListOrdersQuery(1, 10);
        var handler = new ListOrdersHandler(_repositoryMock.Object, _claimsPrincipalMock.Object);

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _repositoryMock
            .Setup(x => x.CountAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Count.ShouldBe(0);
        result.TotalItems.ShouldBe(0);
        result.TotalPages.ShouldBe(0);
        result.HasNextPage.ShouldBeFalse();
        result.HasPreviousPage.ShouldBeFalse();

        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            x => x.CountAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenLastPage_WhenListingOrders_ThenShouldHaveNoNextPage()
    {
        // Arrange
        const int pageIndex = 3;
        const int pageSize = 5;
        const int totalItems = 15;

        var query = new ListOrdersQuery(pageIndex, pageSize);
        var handler = new ListOrdersHandler(_repositoryMock.Object, _claimsPrincipalMock.Object);

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_orders);
        _repositoryMock
            .Setup(x => x.CountAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalItems);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.HasPreviousPage.ShouldBeTrue();
        result.HasNextPage.ShouldBeFalse(); // No next page on last page
        result.TotalPages.ShouldBe(3); // 15 items with page size 5 = 3 pages
        result.PageIndex.ShouldBe(pageIndex);
        result.PageSize.ShouldBe(pageSize);

        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            x => x.CountAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
