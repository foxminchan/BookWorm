using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Features.Orders.Delete;
using BookWorm.Ordering.UnitTests.Fakers;
using BookWorm.SharedKernel.Exceptions;
using MediatR;

namespace BookWorm.Ordering.UnitTests.Features.Orders.Delete;

public sealed class DeleteOrderCommandTests
{
    private readonly DeleteOrderHandler _handler;
    private readonly Order _order;
    private readonly Guid _orderId;
    private readonly Mock<IOrderRepository> _repositoryMock;

    public DeleteOrderCommandTests()
    {
        _order = new OrderFaker().Generate().First();
        _orderId = _order.Id;

        _repositoryMock = new();

        // Setup default mock for UnitOfWork
        _repositoryMock
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenExistingOrder_WhenHandlingDeleteCommand_ThenShouldCallSaveEntitiesAsync()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_order);

        // Act
        var result = await _handler.Handle(new(_orderId), CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
        _order.IsDeleted.ShouldBeTrue();
    }

    [Test]
    public async Task GivenNonExistingOrder_WhenHandlingDeleteCommand_ThenShouldThrowNotFoundException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order)null!);

        // Act
        var act = () => _handler.Handle(new(_orderId), CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Order with id {_orderId} not found.");
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenInvalidId_WhenHandlingDeleteCommand_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var invalidOrderId = Guid.CreateVersion7();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(invalidOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order)null!);

        // Act
        var act = () => _handler.Handle(new(invalidOrderId), CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Order with id {invalidOrderId} not found.");
    }

    [Test]
    public async Task GivenRepositoryThrowsException_WhenHandlingDeleteCommand_ThenShouldPropagateException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var act = () => _handler.Handle(new(_orderId), CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidOperationException>();
    }
}
