using System.Security.Claims;
using BookWorm.Basket.Grpc.Services;
using BookWorm.Catalog.Grpc.Services;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Extensions;
using BookWorm.Ordering.Features.Orders.Create;
using BookWorm.Ordering.Grpc.Services.Basket;
using BookWorm.Ordering.Grpc.Services.Book;
using BookWorm.Ordering.Infrastructure.DistributedLock;

namespace BookWorm.Ordering.UnitTests.Features.Orders.Create;

public sealed class CreateOrderCommandTests
{
    private readonly Mock<IBasketService> _basketServiceMock;
    private readonly Mock<IBookService> _bookServiceMock;
    private readonly Mock<ClaimsPrincipal> _claimsPrincipalMock;
    private readonly CreateOrderCommand _command;
    private readonly CreateOrderHandler _handler;
    private readonly Mock<IDistributedAccessLockProvider> _lockProviderMock;
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly string _userId;

    public CreateOrderCommandTests()
    {
        _userId = Guid.CreateVersion7().ToString();
        _repositoryMock = new();
        _claimsPrincipalMock = new();
        _lockProviderMock = new();
        _basketServiceMock = new();
        _bookServiceMock = new();

        var claim = new Claim(ClaimTypes.NameIdentifier, _userId);
        _claimsPrincipalMock.Setup(x => x.FindFirst(ClaimTypes.NameIdentifier)).Returns(claim);

        SetupDefaultBasketAndBookServices();

        _handler = new(
            _repositoryMock.Object,
            _claimsPrincipalMock.Object,
            _lockProviderMock.Object,
            _basketServiceMock.Object,
            _bookServiceMock.Object
        );

        _command = new();
    }

    [Test]
    public async Task GivenUnauthenticatedUser_WhenHandlingCreateOrder_ThenShouldThrowUnauthorizedException()
    {
        // Arrange
        _claimsPrincipalMock
            .Setup(x => x.FindFirst(ClaimTypes.NameIdentifier))
            .Returns((Claim?)null!);

        // Act
        var act = async () => await _handler.Handle(_command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<UnauthorizedAccessException>();
        exception.Message.ShouldBe("User is not authenticated.");

        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenValidCommand_WhenOrderIsAlreadyLocked_ThenShouldThrowConcurrencyException()
    {
        // Arrange
        var mockLock = new Mock<IDistributedAccessLock>();
        mockLock.Setup(x => x.IsAcquired).Returns(false);

        _lockProviderMock
            .Setup(x =>
                x.TryAcquireAsync(
                    It.IsAny<string>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(mockLock.Object);

        // Act
        var act = async () => await _handler.Handle(_command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Other process is already creating an order");
    }

    [Test]
    public async Task GivenValidCommand_WhenCreatingOrder_ThenShouldReturnOrderId()
    {
        // Arrange
        var expectedOrder = new Order(
            _userId.ToBuyerId(),
            null,
            [new(Guid.CreateVersion7(), 2, 10.99m)]
        );
        var mockLock = new Mock<IDistributedAccessLock>();
        mockLock.Setup(x => x.IsAcquired).Returns(true);
        mockLock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

        _lockProviderMock
            .Setup(x =>
                x.TryAcquireAsync(
                    It.IsAny<string>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(mockLock.Object);

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        _repositoryMock
            .Setup(x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedOrder.Id);
        _repositoryMock.Verify(
            x =>
                x.AddAsync(
                    It.Is<Order>(o => o.BuyerId == _userId.ToBuyerId()),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _repositoryMock.Verify(
            x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
        mockLock.Verify(x => x.DisposeAsync(), Times.Once);
    }

    private void SetupDefaultBasketAndBookServices()
    {
        var basketResponse = new GetBasketResponse
        {
            Id = Guid.CreateVersion7().ToString(),
            Items =
            {
                new Item { Id = Guid.CreateVersion7().ToString(), Quantity = 2 },
                new Item { Id = Guid.CreateVersion7().ToString(), Quantity = 1 },
            },
        };

        var booksResponse = new GetBooksResponse
        {
            Books =
            {
                new GetBookResponse
                {
                    Id = basketResponse.Items[0].Id,
                    Name = "Book 1",
                    Price = 10.99m,
                },
                new GetBookResponse
                {
                    Id = basketResponse.Items[1].Id,
                    Name = "Book 2",
                    Price = 15.50m,
                },
            },
        };

        _basketServiceMock
            .Setup(x => x.GetBasket(It.IsAny<CancellationToken>()))
            .ReturnsAsync(basketResponse);

        _bookServiceMock
            .Setup(x =>
                x.GetBooksByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(booksResponse);
    }
}
