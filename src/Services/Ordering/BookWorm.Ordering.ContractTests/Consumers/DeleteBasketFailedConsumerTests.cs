using BookWorm.Chassis.Repository;
using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.IntegrationEvents.EventHandlers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Ordering.ContractTests.Consumers;

public sealed class DeleteBasketFailedConsumerTests
{
    private const string Email = "test@example.com";
    private const decimal TotalMoney = 100.00m;
    private Guid _basketId;
    private Guid _orderId;
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;
    private Mock<IOrderRepository> _repositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;

    [Before(Test)]
    public async Task SetUpAsync()
    {
        _orderId = Guid.CreateVersion7();
        _basketId = Guid.CreateVersion7();

        _unitOfWorkMock = new();
        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock = new();
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _provider = new ServiceCollection()
            .AddTelemetryListener()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketFailedCommandHandler>())
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        _harness = await _provider.StartTestHarness();
    }

    [After(Test)]
    public async Task TearDownAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
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

        // Act
        await _harness.Bus.Publish(command);

        // Assert
        var consumer = _harness.GetConsumerHarness<DeleteBasketFailedCommandHandler>();
        await consumer.Consumed.Any<DeleteBasketFailedCommand>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenDeleteBasketFailedCommandWithNonExistingOrder_WhenHandling_ThenConsumerShouldHandleGracefully()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new DeleteBasketFailedCommand(_basketId, Email, _orderId, TotalMoney);

        // Act
        await _harness.Bus.Publish(command);

        // Assert
        var consumer = _harness.GetConsumerHarness<DeleteBasketFailedCommandHandler>();
        await consumer.Consumed.Any<DeleteBasketFailedCommand>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(x => x.Delete(It.IsAny<Order>()), Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
