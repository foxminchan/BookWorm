using BookWorm.Chassis.Repository;
using BookWorm.Contracts;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.IntegrationEvents.EventHandlers;
using MassTransit;

namespace BookWorm.Ordering.UnitTests.IntegrationEvents;

public sealed class DeleteBasketFailedCommandHandlerTests
{
    private readonly Mock<ConsumeContext<DeleteBasketFailedCommand>> _contextMock = new();
    private readonly DeleteBasketFailedCommandHandler _handler;
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    public DeleteBasketFailedCommandHandlerTests()
    {
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenOrderExists_WhenConsuming_ThenShouldDeleteOrderAndSave()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var command = new DeleteBasketFailedCommand(
            Guid.CreateVersion7(),
            "user@test.com",
            orderId,
            99.99m
        );
        _contextMock.Setup(x => x.Message).Returns(command);

        var order = new Order(Guid.CreateVersion7(), "Test order", []);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Consume(_contextMock.Object);

        // Assert
        order.IsDeleted.ShouldBeTrue();
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenOrderNotFound_WhenConsuming_ThenShouldReturnWithoutSaving()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var command = new DeleteBasketFailedCommand(
            Guid.CreateVersion7(),
            "user@test.com",
            orderId,
            50.00m
        );
        _contextMock.Setup(x => x.Message).Returns(command);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        await _handler.Consume(_contextMock.Object);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
