using System.Security.Claims;
using BookWorm.Chassis.Exceptions;
using BookWorm.Constants.Core;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Features.Orders;
using BookWorm.Ordering.Features.Orders.GetSummary;

namespace BookWorm.Ordering.UnitTests.Features.Orders.GetSummary;

public sealed class GetOrderSummaryPreProcessorTests
{
    private readonly OrderId _orderId = OrderId.From(Guid.CreateVersion7());
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();

    [Test]
    public async Task GivenAdminRole_WhenProcessing_ThenShouldNotQueryRepository()
    {
        var preProcessor = new GetOrderSummaryPreProcessor(
            _orderRepositoryMock.Object,
            CreateAdminPrincipal()
        );

        await preProcessor.Handle(
            new(_orderId),
            (_, _) => new(new OrderSummaryDto((Guid)_orderId, Status.New, 0m, 0)),
            CancellationToken.None
        );

        _orderRepositoryMock.Verify(
            r => r.AnyAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenBuyerRoleAndOwnedOrder_WhenProcessing_ThenShouldPass()
    {
        var buyerId = Guid.CreateVersion7();
        _orderRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var preProcessor = new GetOrderSummaryPreProcessor(
            _orderRepositoryMock.Object,
            CreateBuyerPrincipal(buyerId)
        );

        await preProcessor.Handle(
            new(_orderId),
            (_, _) => new(new OrderSummaryDto((Guid)_orderId, Status.New, 0m, 0)),
            CancellationToken.None
        );

        _orderRepositoryMock.Verify(
            r => r.AnyAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenBuyerRoleAndUnownedOrder_WhenProcessing_ThenShouldThrowNotFoundException()
    {
        var buyerId = Guid.CreateVersion7();
        _orderRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var preProcessor = new GetOrderSummaryPreProcessor(
            _orderRepositoryMock.Object,
            CreateBuyerPrincipal(buyerId)
        );

        Func<Task> act = async () =>
            await preProcessor.Handle(
                new(_orderId),
                (_, _) => new(new OrderSummaryDto((Guid)_orderId, Status.New, 0m, 0)),
                CancellationToken.None
            );

        await act.ShouldThrowAsync<NotFoundException>();
    }

    private static ClaimsPrincipal CreateAdminPrincipal()
    {
        return new(new ClaimsIdentity([new(ClaimTypes.Role, Authorization.Roles.Admin)], "Test"));
    }

    private static ClaimsPrincipal CreateBuyerPrincipal(Guid buyerId)
    {
        return new(
            new ClaimsIdentity([new(ClaimTypes.NameIdentifier, buyerId.ToString())], "Test")
        );
    }
}
