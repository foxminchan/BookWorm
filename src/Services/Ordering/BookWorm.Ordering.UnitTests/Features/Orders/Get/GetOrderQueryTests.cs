using System.Security.Claims;
using BookWorm.Catalog.Grpc.Services;
using BookWorm.Chassis.Exceptions;
using BookWorm.Constants.Core;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Features.Orders;
using BookWorm.Ordering.Features.Orders.Get;
using BookWorm.Ordering.Grpc.Services.Book;
using BookWorm.Ordering.UnitTests.Fakers;
using BookWorm.ServiceDefaults.Keycloak;

namespace BookWorm.Ordering.UnitTests.Features.Orders.Get;

public sealed class GetOrderQueryTests
{
    private readonly Mock<IBookService> _bookServiceMock;
    private readonly Guid _buyerId;
    private readonly Mock<ClaimsPrincipal> _claimsPrincipalMock;
    private readonly GetOrderHandler _handler;
    private readonly Guid _id;
    private readonly Order _order;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly PostGetOrderHandler _postHandler;

    public GetOrderQueryTests()
    {
        _orderRepositoryMock = new();
        _claimsPrincipalMock = new();
        _bookServiceMock = new();

        _id = Guid.CreateVersion7();
        _buyerId = Guid.CreateVersion7();

        // Create a sample order using the faker
        var orderFaker = new OrderFaker();
        _order = orderFaker.Generate()[0];

        // Replace it with our specific IDs for testing
        _order.GetType().GetProperty("Id")?.SetValue(_order, _id);
        _order.GetType().GetProperty("BuyerId")?.SetValue(_order, _buyerId);

        _handler = new(_orderRepositoryMock.Object, _claimsPrincipalMock.Object);
        _postHandler = new(_bookServiceMock.Object);
    }

    [Test]
    public async Task GivenAdminRole_WhenGettingOrder_ThenShouldRetrieveOrderById()
    {
        // Arrange
        SetupAdminUser();
        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_order);

        // Act
        var result = await _handler.Handle(new(_id), CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(_id);
        _orderRepositoryMock.Verify(
            r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _orderRepositoryMock.Verify(
            r => r.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenRegularUser_WhenGettingOwnOrder_ThenShouldRetrieveOrder()
    {
        // Arrange
        SetupRegularUser(_buyerId.ToString());
        _orderRepositoryMock
            .Setup(r =>
                r.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(_order);

        // Act
        var result = await _handler.Handle(new(_id), CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(_id);
        result.Date.ShouldBe(_order.CreatedAt);
        result.Total.ShouldBe(_order.TotalPrice);
        result.Status.ShouldBe(_order.Status);
        _orderRepositoryMock.Verify(
            r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()),
            Times.Never
        );
        _orderRepositoryMock.Verify(
            r => r.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenRegularUser_WhenGettingOtherUserOrder_ThenShouldThrowNotFoundException()
    {
        // Arrange
        SetupRegularUser(Guid.CreateVersion7().ToString()); // Different user ID
        _orderRepositoryMock
            .Setup(r =>
                r.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Order)null!);

        // Act
        var act = () => _handler.Handle(new(_id), CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task GivenAdminRole_WhenGettingNonExistentOrder_ThenShouldThrowNotFoundException()
    {
        // Arrange
        SetupAdminUser();
        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order)null!);

        // Act
        var act = () => _handler.Handle(new(_id), CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task GivenOrderWithItems_WhenPostProcessing_ThenShouldFillInBookNames()
    {
        // Arrange
        var bookId1 = Guid.CreateVersion7();
        var bookId2 = Guid.CreateVersion7();

        var orderDetailDto = new OrderDetailDto(
            _id,
            DateTime.UtcNow,
            150.00m,
            Status.New,
            new List<OrderItemDto> { new(bookId1, 2, 50.00m), new(bookId2, 1, 50.00m) }
        );

        _bookServiceMock
            .Setup(b => b.GetBookByIdAsync(bookId1.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookResponse { Name = "Book 1" });
        _bookServiceMock
            .Setup(b => b.GetBookByIdAsync(bookId2.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookResponse { Name = "Book 2" });

        // Act
        await _postHandler.Process(new(_id), orderDetailDto, CancellationToken.None);

        // Assert
        orderDetailDto.Items.ShouldNotBeEmpty();
        orderDetailDto.Items.Count.ShouldBe(2);
    }

    [Test]
    public async Task GivenOrderWithItemsAndMissingBook_WhenPostProcessing_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var bookId1 = Guid.CreateVersion7();
        var bookId2 = Guid.CreateVersion7();

        var orderDetailDto = new OrderDetailDto(
            _id,
            DateTime.UtcNow,
            150.00m,
            Status.Cancelled,
            new List<OrderItemDto> { new(bookId1, 2, 50.00m), new(bookId2, 1, 50.00m) }
        );

        _bookServiceMock
            .Setup(b => b.GetBookByIdAsync(bookId1.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookResponse { Name = "Book 1" });
        _bookServiceMock
            .Setup(b => b.GetBookByIdAsync(bookId2.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookResponse)null!);

        // Act
        var act = () => _postHandler.Process(new(_id), orderDetailDto, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    private void SetupAdminUser()
    {
        var roles = new[] { Authorization.Roles.Admin };
        _claimsPrincipalMock
            .Setup(c => c.FindAll(ClaimTypes.Role))
            .Returns(roles.Select(r => new Claim(ClaimTypes.Role, r)));
    }

    private void SetupRegularUser(string userId)
    {
        string[] roles = [];
        _claimsPrincipalMock
            .Setup(c => c.FindAll(ClaimTypes.Role))
            .Returns(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var subjectClaim = new Claim(KeycloakClaimTypes.Subject, userId);
        _claimsPrincipalMock
            .Setup(c => c.FindFirst(KeycloakClaimTypes.Subject))
            .Returns(subjectClaim);
    }
}
