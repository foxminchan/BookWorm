using BookWorm.Contracts;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.IntegrationEvents.EventHandlers;
using BookWorm.SharedKernel.Repository;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Ordering.UnitTests.Consumers;

public sealed class DeleteBasketFailedConsumerTests
{
    private readonly Guid _basketId;
    private readonly string _email;
    private readonly Guid _orderId;
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public DeleteBasketFailedConsumerTests()
    {
        _orderId = Guid.CreateVersion7();
        _basketId = Guid.CreateVersion7();
        _email = "test@example.com";

        _unitOfWorkMock = new();
        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock = new();
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task GivenValidDeleteBasketFailedCommand_WhenHandling_ThenShouldDeleteOrder()
    {
        // Arrange
        var order = new Order(_orderId, default, []);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new DeleteBasketFailedCommand(_basketId, _email, _orderId);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketFailedCommandHandler>())
            .AddScoped<IOrderRepository>(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<DeleteBasketFailedCommandHandler>();

        (await consumerHarness.Consumed.Any<DeleteBasketFailedCommand>()).ShouldBeTrue();

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(x => x.Delete(order), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        await harness.Stop();
    }

    [Test]
    public async Task GivenDeleteBasketFailedCommandWithNonExistingOrder_WhenHandling_ThenShouldReturnGracefully()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order)default!);

        var command = new DeleteBasketFailedCommand(_basketId, _email, _orderId);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketFailedCommandHandler>())
            .AddScoped<IOrderRepository>(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<DeleteBasketFailedCommandHandler>();

        (await consumerHarness.Consumed.Any<DeleteBasketFailedCommand>()).ShouldBeTrue();

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(x => x.Delete(It.IsAny<Order>()), Times.Never);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        await harness.Stop();
    }
}
