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
using BookWorm.SharedKernel.Helpers;

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

        _handler = new(
            _orderRepositoryMock.Object,
            _claimsPrincipalMock.Object,
            _bookServiceMock.Object
        );
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

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(async () =>
            await _handler.Handle(new(_id), CancellationToken.None)
        );
    }

    [Test]
    public async Task GivenAdminRole_WhenGettingNonExistentOrder_ThenShouldThrowNotFoundException()
    {
        // Arrange
        SetupAdminUser();
        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order)null!);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(async () =>
            await _handler.Handle(new(_id), CancellationToken.None)
        );
    }

    [Test]
    public async Task GivenOrderWithItems_WhenBookServiceReturnsBooks_ThenShouldEnrichItemsWithBookNames()
    {
        // Arrange
        SetupAdminUser();

        var orderItems = _order.OrderItems.ToList();
        for (var i = 0; i < orderItems.Count; i++)
        {
            orderItems[i].Id = Guid.CreateVersion7();
        }

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_order);

        var booksResponse = new GetBooksResponse();
        foreach (var item in orderItems)
        {
            booksResponse.Books.Add(
                new GetBookResponse
                {
                    Id = item.Id.ToString(),
                    Name = $"Book for {item.Id}",
                    Price = item.Price,
                }
            );
        }

        _bookServiceMock
            .Setup(x =>
                x.GetBooksByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(booksResponse);

        // Act
        var result = await _handler.Handle(new(_id), CancellationToken.None);

        // Assert
        result.Items.Count.ShouldBe(orderItems.Count);
        foreach (var item in result.Items)
        {
            item.Name.ShouldBe($"Book for {item.Id}");
        }
    }

    [Test]
    public async Task GivenOrderWithItems_WhenBookServiceReturnsPartialMatch_ThenShouldEnrichOnlyMatchedItems()
    {
        // Arrange
        SetupAdminUser();

        var orderItems = _order.OrderItems.ToList();
        for (var i = 0; i < orderItems.Count; i++)
        {
            orderItems[i].Id = Guid.CreateVersion7();
        }

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_order);

        var firstItem = orderItems[0];

        var booksResponse = new GetBooksResponse
        {
            Books =
            {
                new GetBookResponse
                {
                    Id = firstItem.Id.ToString(),
                    Name = "Matched Book",
                    Price = firstItem.Price,
                },
            },
        };

        _bookServiceMock
            .Setup(x =>
                x.GetBooksByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(booksResponse);

        // Act
        var result = await _handler.Handle(new(_id), CancellationToken.None);

        // Assert
        var matchedItem = result.Items.First(i => i.Id == firstItem.Id);
        matchedItem.Name.ShouldBe("Matched Book");

        var unmatchedItems = result.Items.Where(i => i.Id != firstItem.Id);
        foreach (var item in unmatchedItems)
        {
            item.Name.ShouldBeNull();
        }
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

        var subjectClaim = new Claim(ClaimTypes.NameIdentifier, userId);
        _claimsPrincipalMock
            .Setup(c => c.FindFirst(ClaimTypes.NameIdentifier))
            .Returns(subjectClaim);
    }
}
