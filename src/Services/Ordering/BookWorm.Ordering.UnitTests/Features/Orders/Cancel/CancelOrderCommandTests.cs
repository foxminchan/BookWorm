using BookWorm.Chassis.Exceptions;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Features.Orders.Cancel;
using BookWorm.Ordering.UnitTests.Fakers;

namespace BookWorm.Ordering.UnitTests.Features.Orders.Cancel;

public sealed class CancelOrderCommandTests
{
    private readonly CancelOrderHandler _handler;
    private readonly Order _order;
    private readonly Guid _orderId;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;

    public CancelOrderCommandTests()
    {
        _orderRepositoryMock = new();
        _handler = new(_orderRepositoryMock.Object);

        // Create a test order using the OrderFaker
        _order = new OrderFaker().Generate(1)[0];
        _orderId = _order.Id;
    }

    [Test]
    public async Task GivenValidOrderId_WhenCancellingOrder_ThenOrderStatusShouldBeCancelled()
    {
        // Arrange
        var command = new CancelOrderCommand(_orderId);

        _orderRepositoryMock
            .Setup(r =>
                r.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(_order);

        _orderRepositoryMock
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _order.Status.ShouldBe(Status.Cancelled);
        _orderRepositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNonExistingOrderId_WhenCancellingOrder_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var command = new CancelOrderCommand(Guid.CreateVersion7());

        _orderRepositoryMock
            .Setup(r =>
                r.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Order)null!);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
        _orderRepositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenAlreadyCancelledOrder_WhenCancellingOrder_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var cancelledOrder = new OrderFaker().Generate(1)[0];
        typeof(Order).GetProperty(nameof(Order.Status))!.SetValue(cancelledOrder, Status.Cancelled);

        var command = new CancelOrderCommand(cancelledOrder.Id);

        _orderRepositoryMock
            .Setup(r =>
                r.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Order?)default!);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Order with id {cancelledOrder.Id} not found.");
        _orderRepositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenCompletedOrder_WhenCancellingOrder_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var completedOrder = new OrderFaker().Generate(1)[0];
        typeof(Order).GetProperty(nameof(Order.Status))!.SetValue(completedOrder, Status.Completed);

        var command = new CancelOrderCommand(completedOrder.Id);

        _orderRepositoryMock
            .Setup(r =>
                r.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Order?)default!);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Order with id {completedOrder.Id} not found.");
        _orderRepositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
