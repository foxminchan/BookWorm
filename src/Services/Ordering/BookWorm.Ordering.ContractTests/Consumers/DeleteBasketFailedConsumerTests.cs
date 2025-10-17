﻿using BookWorm.Chassis.Repository;
using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.IntegrationEvents.EventHandlers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Ordering.ContractTests.Consumers;

public sealed class DeleteBasketFailedConsumerTests : SnapshotTestBase
{
    private readonly Guid _basketId;
    private readonly string _email;
    private readonly Guid _orderId;
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly decimal _totalMoney;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public DeleteBasketFailedConsumerTests()
    {
        _orderId = Guid.CreateVersion7();
        _basketId = Guid.CreateVersion7();
        _email = "test@example.com";
        _totalMoney = 100.00m;

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

        var command = new DeleteBasketFailedCommand(_basketId, _email, _orderId, _totalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketFailedCommandHandler>())
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<DeleteBasketFailedCommandHandler>();

        (await consumerHarness.Consumed.Any<DeleteBasketFailedCommand>()).ShouldBeTrue();

        var consumedMessage = consumerHarness.Consumed.Select<DeleteBasketFailedCommand>().First();

        // Verify the consumed command contract structure
        await VerifySnapshot(
            new
            {
                CommandType = nameof(DeleteBasketFailedCommand),
                Properties = new
                {
                    consumedMessage.Context.Message.BasketId,
                    consumedMessage.Context.Message.Email,
                    consumedMessage.Context.Message.OrderId,
                    consumedMessage.Context.Message.TotalMoney,
                },
                Schema = new
                {
                    BasketIdType = consumedMessage.Context.Message.BasketId.GetType().Name,
                    EmailType = consumedMessage.Context.Message.Email?.GetType().Name,
                    OrderIdType = consumedMessage.Context.Message.OrderId.GetType().Name,
                    TotalMoneyType = consumedMessage.Context.Message.TotalMoney.GetType().Name,
                    HasId = consumedMessage.Context.Message.Id != Guid.Empty,
                    HasCreationDate = consumedMessage.Context.Message.CreationDate != default,
                    IsIntegrationEvent = true,
                },
            }
        );

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(x => x.Delete(order), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);

        await harness.Stop();
    }

    [Test]
    public async Task GivenDeleteBasketFailedCommandWithNonExistingOrder_WhenHandling_ThenConsumerShouldHandleGracefully()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new DeleteBasketFailedCommand(_basketId, _email, _orderId, _totalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketFailedCommandHandler>())
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<DeleteBasketFailedCommandHandler>();

        (await consumerHarness.Consumed.Any<DeleteBasketFailedCommand>()).ShouldBeTrue();

        var consumedMessage = consumerHarness.Consumed.Select<DeleteBasketFailedCommand>().First();

        // Verify the consumed command contract structure
        await VerifySnapshot(
            new
            {
                CommandType = nameof(DeleteBasketFailedCommand),
                Properties = new
                {
                    consumedMessage.Context.Message.BasketId,
                    consumedMessage.Context.Message.Email,
                    consumedMessage.Context.Message.OrderId,
                    consumedMessage.Context.Message.TotalMoney,
                },
                Schema = new
                {
                    BasketIdType = consumedMessage.Context.Message.BasketId.GetType().Name,
                    EmailType = consumedMessage.Context.Message.Email?.GetType().Name,
                    OrderIdType = consumedMessage.Context.Message.OrderId.GetType().Name,
                    TotalMoneyType = consumedMessage.Context.Message.TotalMoney.GetType().Name,
                    HasId = consumedMessage.Context.Message.Id != Guid.Empty,
                    HasCreationDate = consumedMessage.Context.Message.CreationDate != default,
                    IsIntegrationEvent = true,
                },
            }
        );

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_orderId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(x => x.Delete(It.IsAny<Order>()), Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );

        await harness.Stop();
    }
}
