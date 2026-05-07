using BookWorm.Chassis.Repository;
using BookWorm.Contracts;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.IntegrationEvents.EventHandlers;

namespace BookWorm.Ordering.UnitTests.IntegrationEvents;

public sealed class DeleteBasketFailedCommandHandlerTests
{
    private readonly DeleteBasketFailedCommandHandler _handler;
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    public DeleteBasketFailedCommandHandlerTests()
    {
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenOrderExists_WhenHandling_ThenShouldDeleteOrderAndSave()
    {
        var orderId = Guid.CreateVersion7();
        var command = new DeleteBasketFailedCommand(
            Guid.CreateVersion7(),
            "user@test.com",
            orderId,
            99.99m
        );

        var order = new Order(Guid.CreateVersion7(), "Test order", []);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _handler.Handle(command, CancellationToken.None);

        order.IsDeleted.ShouldBeTrue();
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenOrderNotFound_WhenHandling_ThenShouldReturnWithoutSaving()
    {
        var orderId = Guid.CreateVersion7();
        var command = new DeleteBasketFailedCommand(
            Guid.CreateVersion7(),
            "user@test.com",
            orderId,
            50.00m
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        await _handler.Handle(command, CancellationToken.None);

        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
