using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Ordering.Domain.OrderAggregate.Specifications;
using BookWorm.Ordering.Features.Orders.List;

namespace BookWorm.Ordering.UnitTests.Application.Orders;

public sealed class ListOrdersHandlerTests
{
    private readonly ListOrdersHandler _handler;
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<IReadRepository<Order>> _repositoryMock = new();

    public ListOrdersHandlerTests()
    {
        _handler = new(_repositoryMock.Object, _identityServiceMock.Object);
    }

    [Fact]
    public async Task GivenAdminRole_HandleAsync_ReturnsOrders()
    {
        // Arrange
        _identityServiceMock.Setup(x => x.IsAdminRole()).Returns(true);
        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(Guid.NewGuid().ToString());
        _repositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var query = new ListOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenNotAdminRole_HandleAsync_ReturnsOrders()
    {
        // Arrange
        _identityServiceMock.Setup(x => x.IsAdminRole()).Returns(false);
        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(Guid.NewGuid().ToString());
        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new ListOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenNotAdminRoleAndEmptyCustomerId_ShouldThrowsException()
    {
        // Arrange
        _identityServiceMock.Setup(x => x.IsAdminRole()).Returns(false);
        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(string.Empty);

        var query = new ListOrdersQuery();

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
