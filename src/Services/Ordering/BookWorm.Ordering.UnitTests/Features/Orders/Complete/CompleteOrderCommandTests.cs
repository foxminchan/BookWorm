using BookWorm.Chassis.Exceptions;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Features.Orders.Complete;
using BookWorm.Ordering.UnitTests.Fakers;

namespace BookWorm.Ordering.UnitTests.Features.Orders.Complete;

public sealed class CompleteOrderCommandTests
{
    private readonly CompleteOrderHandler _handler;
    private readonly Order _order;
    private readonly Guid _orderId;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;

    public CompleteOrderCommandTests()
    {
        _orderRepositoryMock = new();
        _handler = new(_orderRepositoryMock.Object);

        // Create a test order using the OrderFaker
        _order = new OrderFaker().Generate(1)[0];
        _orderId = _order.Id;
    }

    [Test]
    public async Task GivenValidOrderId_WhenCompletingOrder_ThenOrderStatusShouldBeCompleted()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId);

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
        _order.Status.ShouldBe(Status.Completed);
        _orderRepositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNonExistingOrderId_WhenCompletingOrder_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var command = new CompleteOrderCommand(Guid.CreateVersion7());

        _orderRepositoryMock
            .Setup(r =>
                r.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Order)null!);

        // Act
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _handler.Handle(command, CancellationToken.None)
        );

        // Assert
        exception.Message.ShouldBe($"Order with id {command.OrderId} not found.");
        _orderRepositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenOrderAlreadyCompleted_WhenCompletingOrder_ThenShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId);

        typeof(Order).GetProperty(nameof(Order.Status))?.SetValue(_order, Status.Completed);

        _orderRepositoryMock
            .Setup(r =>
                r.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Order?)null!);

        // Act
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _handler.Handle(command, CancellationToken.None)
        );

        // Assert
        exception.Message.ShouldBe($"Order with id {_orderId} not found.");
        _orderRepositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenOrderAlreadyCancelled_WhenCompletingOrder_ThenShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CompleteOrderCommand(_orderId);

        typeof(Order).GetProperty(nameof(Order.Status))?.SetValue(_order, Status.Cancelled);

        _orderRepositoryMock
            .Setup(r =>
                r.FirstOrDefaultAsync(It.IsAny<OrderFilterSpec>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Order?)null!);

        // Act
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _handler.Handle(command, CancellationToken.None)
        );

        // Assert
        exception.Message.ShouldBe($"Order with id {_orderId} not found.");
        _orderRepositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
