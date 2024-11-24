using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Ordering.Domain.OrderAggregate.Specifications;
using BookWorm.Ordering.Features.Orders.Get;
using BookWorm.Ordering.Grpc;
using BookWorm.Ordering.UnitTests.Builder;

namespace BookWorm.Ordering.UnitTests.Application.Orders;

public sealed class GetOrderHandlerTests
{
    private readonly Mock<IBookService> _bookServiceMock;
    private readonly GetOrderHandler _handler;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IReadRepository<Order>> _repositoryMock;

    public GetOrderHandlerTests()
    {
        _repositoryMock = new();
        _identityServiceMock = new();
        _bookServiceMock = new();
        _handler = new(
            _repositoryMock.Object,
            _identityServiceMock.Object,
            _bookServiceMock.Object
        );
    }

    [Fact]
    public async Task GivenValidOrderIdAndCustomer_ShouldReturnOrderDetail()
    {
        // Arrange
        var order = OrderBuilder.WithDefaultValues()[0];
        var orderId = order.Id;

        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(Guid.NewGuid().ToString());
        _identityServiceMock.Setup(x => x.IsAdminRole()).Returns(false);
        _repositoryMock
            .Setup(x =>
                x.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(order);
        _bookServiceMock
            .Setup(x => x.GetBookAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookItem(Guid.NewGuid(), "Name"));

        var query = new GetOrderQuery(orderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value.Id.Should().Be(orderId);
        result.Value.Note.Should().Be("Note 1");
        _repositoryMock.Verify(
            x => x.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _identityServiceMock.Verify(x => x.GetUserIdentity(), Times.Once);
    }

    [Fact]
    public async Task GivenAdminRole_ShouldReturnOrderDetail()
    {
        // Arrange
        var order = OrderBuilder.WithDefaultValues()[0];
        var orderId = order.Id;

        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(Guid.NewGuid().ToString());
        _identityServiceMock.Setup(x => x.IsAdminRole()).Returns(true);
        _repositoryMock
            .Setup(x =>
                x.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(order);
        _bookServiceMock
            .Setup(x => x.GetBookAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookItem(Guid.NewGuid(), "Name"));

        var query = new GetOrderQuery(orderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value.Id.Should().Be(orderId);
        result.Value.Note.Should().Be("Note 1");
        _repositoryMock.Verify(
            x => x.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _identityServiceMock.Verify(x => x.GetUserIdentity(), Times.Once);
    }

    [Fact]
    public async Task GivenNonExistentOrder_ShouldBeReturnNull()
    {
        // Arrange
        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(Guid.NewGuid().ToString());
        _identityServiceMock.Setup(x => x.IsAdminRole()).Returns(false);
        _repositoryMock
            .Setup(x =>
                x.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Order?)null);

        var query = new GetOrderQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
        _bookServiceMock.Verify(
            x => x.GetBookAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _repositoryMock.Verify(
            x => x.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _identityServiceMock.Verify(x => x.GetUserIdentity(), Times.Once);
    }

    [Theory]
    [CombinatorialData]
    public async Task GivenInvalidCustomerIdOrOrderId_WhenHandleIsCalled_ThenThrowArgumentNullException(
        [CombinatorialValues(null, "")] string? customerId
    )
    {
        // Arrange
        var query = new GetOrderQuery(Guid.NewGuid());
        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(customerId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        if (customerId is null)
        {
            await act.Should().ThrowAsync<ArgumentNullException>();
        }
        else
        {
            await act.Should().ThrowAsync<ArgumentException>();
        }

        _bookServiceMock.Verify(
            x => x.GetBookAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _repositoryMock.Verify(
            x => x.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _identityServiceMock.Verify(x => x.GetUserIdentity(), Times.Once);
    }
}
