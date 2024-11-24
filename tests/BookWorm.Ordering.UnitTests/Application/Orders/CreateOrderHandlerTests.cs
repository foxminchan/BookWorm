using BookWorm.Contracts;
using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Ordering.Features.Orders.Create;
using BookWorm.Ordering.Grpc;

namespace BookWorm.Ordering.UnitTests.Application.Orders;

public sealed class CreateOrderHandlerTests
{
    private readonly Mock<IBasketService> _basketServiceMock = new();

    private readonly CreateOrderHandler _handler;
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<IRepository<Order>> _repositoryMock = new();

    public CreateOrderHandlerTests()
    {
        _handler = new(
            _repositoryMock.Object,
            _basketServiceMock.Object,
            _identityServiceMock.Object,
            _publishEndpointMock.Object
        );
    }

    [Fact]
    public async Task GivenNullBuyerId_ShouldThrowArgumentException_WhenOrderIsCreated()
    {
        // Arrange
        var request = new CreateOrderCommand("note");
        _identityServiceMock.Setup(s => s.GetUserIdentity()).Returns((string)null!);

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<OrderCreatedIntegrationEvent>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Theory]
    [CombinatorialData]
    public async Task GivenValidBasketAndBuyerId_ShouldReturnOrderId_WhenOrderIsCreated(
        [CombinatorialValues(true, false)] bool isBasketNull
    )
    {
        // Arrange
        var basket = isBasketNull
            ? null
            : new Grpc.Basket(
                Guid.NewGuid(),
                new List<BasketItem>
                {
                    new(Guid.NewGuid(), "Book 1", 2, 10.0m, 8.0m),
                    new(Guid.NewGuid(), "Book 2", 1, 20.0m, 18.0m),
                },
                26.0m
            );

        _identityServiceMock.Setup(s => s.GetUserIdentity()).Returns(Guid.NewGuid().ToString());
        _basketServiceMock
            .Setup(s => s.GetBasketAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(basket);
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Order(Guid.NewGuid(), "note"));
        _identityServiceMock.Setup(s => s.GetEmail()).Returns("test@example.com");

        var request = new CreateOrderCommand("note");

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        if (isBasketNull)
        {
            await act.Should().ThrowAsync<ArgumentNullException>();
            _repositoryMock.Verify(
                r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
            _publishEndpointMock.Verify(
                p =>
                    p.Publish(
                        It.IsAny<OrderCreatedIntegrationEvent>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }
        else
        {
            await act.Should().NotThrowAsync();
            _repositoryMock.Verify(
                r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
            _publishEndpointMock.Verify(
                p =>
                    p.Publish(
                        It.IsAny<OrderCreatedIntegrationEvent>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }
    }
}
