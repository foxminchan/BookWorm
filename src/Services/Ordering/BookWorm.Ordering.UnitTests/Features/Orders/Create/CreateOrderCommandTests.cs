using System.Security.Claims;
using BookWorm.Basket.Grpc.Services;
using BookWorm.Catalog.Grpc.Services;
using BookWorm.Chassis.Command;
using BookWorm.Chassis.Exceptions;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Features.Orders.Create;
using BookWorm.Ordering.Grpc.Services.Basket;
using BookWorm.Ordering.Grpc.Services.Book;
using BookWorm.Ordering.Infrastructure.Helpers;
using BookWorm.Ordering.Infrastructure.Services;
using BookWorm.Ordering.UnitTests.Mocks;
using BookWorm.ServiceDefaults.Keycloak;
using Medallion.Threading;

namespace BookWorm.Ordering.UnitTests.Features.Orders.Create;

public sealed class CreateOrderCommandTests
{
    #region PreCreateOrderHandler Tests

    public class PreCreateOrderHandlerTests
    {
        private readonly Mock<IBasketService> _basketServiceMock;
        private readonly Mock<IBookService> _bookServiceMock;
        private readonly CreateOrderCommand _command;
        private readonly PreCreateOrderHandler _handler;

        public PreCreateOrderHandlerTests()
        {
            _basketServiceMock = new();
            _bookServiceMock = new();

            var basketMetadata = new BasketMetadata(
                _basketServiceMock.Object,
                _bookServiceMock.Object
            );

            _handler = new(basketMetadata);
            _command = new();
        }

        [Test]
        public async Task GivenValidBasketItems_WhenProcessing_ThenShouldPopulateOrderItems()
        {
            // Arrange
            var basketResponse = new BasketResponse
            {
                Id = Guid.CreateVersion7().ToString(),
                Items =
                {
                    new Item { Id = Guid.CreateVersion7().ToString(), Quantity = 2 },
                    new Item { Id = Guid.CreateVersion7().ToString(), Quantity = 1 },
                },
            };

            var booksResponse = new BooksResponse
            {
                Books =
                {
                    new BookResponse
                    {
                        Id = basketResponse.Items[0].Id,
                        Name = "Book 1",
                        Price = 10.99,
                    },
                    new BookResponse
                    {
                        Id = basketResponse.Items[1].Id,
                        Name = "Book 2",
                        Price = 12.99,
                        PriceSale = 10.99,
                    },
                },
            };

            _basketServiceMock
                .Setup(x => x.GetBasket(It.IsAny<CancellationToken>()))
                .ReturnsAsync(basketResponse);

            _bookServiceMock
                .Setup(x =>
                    x.GetBooksByIdsAsync(
                        It.Is<IEnumerable<string>>(ids =>
                            new HashSet<string>(ids).SetEquals(
                                new[] { basketResponse.Items[0].Id, basketResponse.Items[1].Id }
                            )
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(booksResponse);

            // Act
            await _handler.Process(_command, CancellationToken.None);

            // Assert
            _command.Items.Count.ShouldBe(2);

            var book1Item = _command.Items.First(i =>
                i.BookId == Guid.Parse(booksResponse.Books[0].Id)
            );
            book1Item.Quantity.ShouldBe(2);
            book1Item.Price.ShouldBe(10.99m);

            var book2Item = _command.Items.First(i =>
                i.BookId == Guid.Parse(booksResponse.Books[1].Id)
            );
            book2Item.Quantity.ShouldBe(1);
            book2Item.Price.ShouldBe(10.99m);
        }

        [Test]
        public async Task GivenMissingBook_WhenProcessing_ThenShouldThrowNotFoundException()
        {
            // Arrange
            var basketResponse = new BasketResponse
            {
                Id = "user123",
                Items =
                {
                    new Item { Id = "book1", Quantity = 2 },
                },
            };

            var booksResponse = new BooksResponse(); // Empty response, no books found

            _basketServiceMock
                .Setup(x => x.GetBasket(It.IsAny<CancellationToken>()))
                .ReturnsAsync(basketResponse);

            _bookServiceMock
                .Setup(x =>
                    x.GetBooksByIdsAsync(
                        It.Is<IEnumerable<string>>(ids => ids.Contains("book1")),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(booksResponse);

            // Act
            var act = async () => await _handler.Process(_command, CancellationToken.None);

            // Assert
            var exception = await act.ShouldThrowAsync<NotFoundException>();
            exception.Message.ShouldBe("Book with id book1 not found.");
        }
    }

    #endregion

    #region CreateOrderHandler Tests

    public class CreateOrderHandlerTests
    {
        private readonly Mock<ClaimsPrincipal> _claimsPrincipalMock;
        private readonly CreateOrderCommand _command;
        private readonly CreateOrderHandler _handler;
        private readonly Mock<IDistributedLockProvider> _lockProviderMock;
        private readonly Mock<IOrderRepository> _repositoryMock;
        private readonly string _userId;

        public CreateOrderHandlerTests()
        {
            _userId = Guid.CreateVersion7().ToString();
            _repositoryMock = new();
            _claimsPrincipalMock = new();
            _lockProviderMock = new();

            var claim = new Claim(KeycloakClaimTypes.Subject, _userId);
            _claimsPrincipalMock.Setup(x => x.FindFirst(KeycloakClaimTypes.Subject)).Returns(claim);

            _handler = new(
                _repositoryMock.Object,
                _claimsPrincipalMock.Object,
                _lockProviderMock.Object
            );

            _command = new()
            {
                Items =
                [
                    new(Guid.CreateVersion7(), 2, 10.99m),
                    new(Guid.CreateVersion7(), 1, 15.50m),
                ],
            };
        }

        [Test]
        public void GivenCreateOrderCommand_WhenCreating_ThenShouldBeOfCorrectType()
        {
            // Act
            var command = new CreateOrderCommand();

            // Assert
            command.ShouldNotBeNull();
            command.ShouldBeOfType<CreateOrderCommand>();
            command.ShouldBeAssignableTo<ICommand<Guid>>();
        }

        [Test]
        public async Task GivenUnauthenticatedUser_WhenHandlingCreateOrder_ThenShouldThrowUnauthorizedException()
        {
            // Arrange
            _claimsPrincipalMock
                .Setup(x => x.FindFirst(KeycloakClaimTypes.Subject))
                .Returns((Claim)default!);

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
            var expectedOrder = new Order(_userId.ToBuyerId(), null, _command.Items);

            _lockProviderMock
                .Setup(x => x.CreateLock(It.IsAny<string>()))
                .Returns(Mock.Of<IDistributedLock>());

            _repositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOrder);

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
            var expectedOrder = new Order(_userId.ToBuyerId(), null, _command.Items);
            var mockLock = new Mock<IDistributedSynchronizationHandle>();
            mockLock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

            var lockProvider = new DistributedLockProviderMock(mockLock.Object);
            var handler = new CreateOrderHandler(
                _repositoryMock.Object,
                _claimsPrincipalMock.Object,
                lockProvider
            );

            _repositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOrder);

            // Act
            var result = await handler.Handle(_command, CancellationToken.None);

            // Assert
            result.ShouldBe(expectedOrder.Id);
            _repositoryMock.Verify(
                x =>
                    x.AddAsync(
                        It.Is<Order>(o =>
                            o.BuyerId == _userId.ToBuyerId()
                            && o.OrderItems.Count == _command.Items.Count
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
            mockLock.Verify(x => x.DisposeAsync(), Times.Once);
        }
    }

    #endregion
}
