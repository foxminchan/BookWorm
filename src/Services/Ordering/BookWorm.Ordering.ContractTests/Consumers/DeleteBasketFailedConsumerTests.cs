using BookWorm.Chassis.Repository;
using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.IntegrationEvents.EventHandlers;

namespace BookWorm.Ordering.ContractTests.Consumers;

public sealed class DeleteBasketFailedConsumerTests
{
    private const string Email = "test@example.com";
    private const decimal TotalMoney = 100.00m;
    private Guid _basketId;
    private Guid _orderId;
    private Mock<IOrderRepository> _repositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;

    [Before(Test)]
    public void SetUp()
    {
        _orderId = Guid.CreateVersion7();
        _basketId = Guid.CreateVersion7();

        _unitOfWorkMock = new();
        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock = new();
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task GivenDeleteBasketFailedCommand_WhenPublished_ThenConsumerShouldConsumeItAndDeleteOrder()
    {
        // Arrange
        var order = new Order(_orderId, null, []);
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new DeleteBasketFailedCommand(_basketId, Email, _orderId, TotalMoney);
        var handler = new DeleteBasketFailedCommandHandler(_repositoryMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.VerifyCloudEvent(command);
        _repositoryMock.Verify(
            x => x.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenOrderNotFound_WhenHandlingDeleteBasketFailed_ThenShouldDoNothing()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new DeleteBasketFailedCommand(_basketId, Email, _orderId, TotalMoney);
        var handler = new DeleteBasketFailedCommandHandler(_repositoryMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.VerifyCloudEvent(command);
        _repositoryMock.Verify(
            x => x.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
