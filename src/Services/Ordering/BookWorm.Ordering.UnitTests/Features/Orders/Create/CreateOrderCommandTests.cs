using System.Security.Claims;
using BookWorm.Basket.Grpc.Services;
using BookWorm.Catalog.Grpc.Services;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Features.Orders.Create;
using BookWorm.Ordering.Grpc.Services.Basket;
using BookWorm.Ordering.Grpc.Services.Book;
using BookWorm.Ordering.Infrastructure.Services;
using BookWorm.ServiceDefaults.Keycloak;
using BookWorm.SharedKernel.Exceptions;

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
                Id = Guid.NewGuid().ToString(),
                Items =
                {
                    new Item { Id = Guid.NewGuid().ToString(), Quantity = 2 },
                    new Item { Id = Guid.NewGuid().ToString(), Quantity = 1 },
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
            exception.Message.ShouldBe("Book with id book1 not found");
        }
    }

    #endregion

    #region CreateOrderHandler Tests

    public class CreateOrderHandlerTests
    {
        private readonly Mock<ClaimsPrincipal> _claimsPrincipalMock;
        private readonly CreateOrderCommand _command;
        private readonly CreateOrderHandler _handler;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly string _userId;

        public CreateOrderHandlerTests()
        {
            _userId = Guid.NewGuid().ToString();
            _orderRepositoryMock = new();
            _claimsPrincipalMock = new();

            var claim = new Claim(KeycloakClaimTypes.Subject, _userId);
            _claimsPrincipalMock.Setup(x => x.FindFirst(KeycloakClaimTypes.Subject)).Returns(claim);

            _handler = new(_orderRepositoryMock.Object, _claimsPrincipalMock.Object);

            _command = new()
            {
                Items = [new(Guid.NewGuid(), 2, 10.99m), new(Guid.NewGuid(), 1, 15.50m)],
            };
        }

        [Test]
        public async Task GivenValidCommand_WhenHandlingCreateOrder_ThenShouldCreateAndReturnOrderId()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var createdOrder = new Order(Guid.Parse(_userId), null, _command.Items)
            {
                Id = orderId,
            };

            _orderRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdOrder);

            // Act
            var result = await _handler.Handle(_command, CancellationToken.None);

            // Assert
            result.ShouldBe(orderId);
            _orderRepositoryMock.Verify(
                x =>
                    x.AddAsync(
                        It.Is<Order>(o =>
                            o.BuyerId == Guid.Parse(_userId) && o.OrderItems.Count == 2
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        [Test]
        public async Task GivenUnauthenticatedUser_WhenHandlingCreateOrder_ThenShouldThrowUnauthorizedException()
        {
            // Arrange
            _claimsPrincipalMock
                .Setup(x => x.FindFirst(KeycloakClaimTypes.Subject))
                .Returns((Claim)null!);

            // Act
            var act = async () => await _handler.Handle(_command, CancellationToken.None);

            // Assert
            var exception = await act.ShouldThrowAsync<UnauthorizedAccessException>();
            exception.Message.ShouldBe("User is not authenticated");

            _orderRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
    }

    #endregion
}
