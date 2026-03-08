using System.Security.Claims;
using BookWorm.Basket.Grpc.Services;
using BookWorm.Catalog.Grpc.Services;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Extensions;
using BookWorm.Ordering.Features.Orders.Create;
using BookWorm.Ordering.Grpc.Services.Basket;
using BookWorm.Ordering.Grpc.Services.Book;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion.Locking.Distributed;

namespace BookWorm.Ordering.UnitTests.Features.Orders.Create;

public sealed class CreateOrderCommandTests
{
    private readonly Mock<IBasketService> _basketServiceMock;
    private readonly Mock<IBookService> _bookServiceMock;
    private readonly Mock<ClaimsPrincipal> _claimsPrincipalMock;
    private readonly CreateOrderCommand _command;
    private readonly Mock<IFusionCacheDistributedLocker> _distributedLockerMock;
    private readonly CreateOrderHandler _handler;
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly string _userId;

    public CreateOrderCommandTests()
    {
        _userId = Guid.CreateVersion7().ToString();
        _repositoryMock = new();
        _claimsPrincipalMock = new();
        _distributedLockerMock = new();
        _basketServiceMock = new();
        _bookServiceMock = new();

        var claim = new Claim(ClaimTypes.NameIdentifier, _userId);
        _claimsPrincipalMock.Setup(x => x.FindFirst(ClaimTypes.NameIdentifier)).Returns(claim);

        SetupDefaultBasketAndBookServices();

        _handler = new(
            _repositoryMock.Object,
            _claimsPrincipalMock.Object,
            _distributedLockerMock.Object,
            Mock.Of<ILogger<CreateOrderHandler>>(),
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
        _distributedLockerMock
            .Setup(x =>
                x.AcquireLockAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<ILogger>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((object?)null);

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
        var lockHandle = new object();

        _distributedLockerMock
            .Setup(x =>
                x.AcquireLockAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<ILogger>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(lockHandle);

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
        _distributedLockerMock.Verify(
            x =>
                x.ReleaseLockAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    lockHandle,
                    It.IsAny<ILogger>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        _bookServiceMock.Verify(
            x =>
                x.GetBooksByIdsAsync(
                    It.Is<IEnumerable<string>>(ids => ids.Count() == 2),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
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
