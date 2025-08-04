using BookWorm.Basket.Domain;
using BookWorm.Basket.IntegrationEvents.EventHandlers;
using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Basket.UnitTests.Consumers;

public sealed class PlaceOrderConsumerTests : SnapshotTestBase
{
    private readonly Guid _basketId;
    private readonly PlaceOrderCommand _command;
    private readonly Mock<IBasketRepository> _repositoryMock;

    public PlaceOrderConsumerTests()
    {
        var orderId = Guid.CreateVersion7();
        _basketId = Guid.CreateVersion7();
        const string email = "test@example.com";
        const string fullName = "Test User";
        const decimal totalMoney = 99.99m;
        _repositoryMock = new();
        _command = new(_basketId, fullName, email, orderId, totalMoney);
    }

    [Test]
    public async Task GivenValidPlaceOrderCommand_WhenBasketDeleteSucceeds_ThenShouldPublishCompleteEvent()
    {
        // Arrange
        _repositoryMock.Setup(x => x.DeleteBasketAsync(_basketId.ToString())).ReturnsAsync(true);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<PlaceOrderCommandHandler>())
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(_command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<PlaceOrderCommandHandler>();

        (await consumerHarness.Consumed.Any<PlaceOrderCommand>()).ShouldBeTrue();
        (await harness.Published.Any<BasketDeletedCompleteIntegrationEvent>()).ShouldBeTrue();
        (await harness.Published.Any<BasketDeletedFailedIntegrationEvent>()).ShouldBeFalse();

        var publishedMessage = harness
            .Published.Select<BasketDeletedCompleteIntegrationEvent>()
            .First();

        // Verify contract structure and properties
        await VerifySnapshot(
            new
            {
                EventType = nameof(BasketDeletedCompleteIntegrationEvent),
                Properties = new
                {
                    publishedMessage.Context.Message.OrderId,
                    publishedMessage.Context.Message.BasketId,
                    publishedMessage.Context.Message.TotalMoney,
                },
                Schema = new
                {
                    OrderIdType = publishedMessage.Context.Message.OrderId.GetType().Name,
                    BasketIdType = publishedMessage.Context.Message.BasketId.GetType().Name,
                    TotalMoneyType = publishedMessage.Context.Message.TotalMoney.GetType().Name,
                    HasId = publishedMessage.Context.Message.Id != Guid.Empty,
                    HasCreationDate = publishedMessage.Context.Message.CreationDate != default,
                },
            }
        );

        _repositoryMock.Verify(x => x.DeleteBasketAsync(_basketId.ToString()), Times.Once);

        await harness.Stop();
    }

    [Test]
    public async Task GivenValidPlaceOrderCommand_WhenBasketDeleteFails_ThenShouldPublishFailedEvent()
    {
        // Arrange
        _repositoryMock.Setup(x => x.DeleteBasketAsync(_basketId.ToString())).ReturnsAsync(false);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<PlaceOrderCommandHandler>())
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(_command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<PlaceOrderCommandHandler>();

        (await consumerHarness.Consumed.Any<PlaceOrderCommand>()).ShouldBeTrue();
        (await harness.Published.Any<BasketDeletedFailedIntegrationEvent>()).ShouldBeTrue();
        (await harness.Published.Any<BasketDeletedCompleteIntegrationEvent>()).ShouldBeFalse();

        var publishedMessage = harness
            .Published.Select<BasketDeletedFailedIntegrationEvent>()
            .First();

        // Verify contract structure and properties
        await VerifySnapshot(
            new
            {
                EventType = nameof(BasketDeletedFailedIntegrationEvent),
                Properties = new
                {
                    publishedMessage.Context.Message.OrderId,
                    publishedMessage.Context.Message.BasketId,
                    publishedMessage.Context.Message.Email,
                    publishedMessage.Context.Message.TotalMoney,
                },
                Schema = new
                {
                    OrderIdType = publishedMessage.Context.Message.OrderId.GetType().Name,
                    BasketIdType = publishedMessage.Context.Message.BasketId.GetType().Name,
                    EmailType = publishedMessage.Context.Message.Email?.GetType().Name,
                    TotalMoneyType = publishedMessage.Context.Message.TotalMoney.GetType().Name,
                    HasId = publishedMessage.Context.Message.Id != Guid.Empty,
                    HasCreationDate = publishedMessage.Context.Message.CreationDate != default,
                },
            }
        );

        _repositoryMock.Verify(x => x.DeleteBasketAsync(_basketId.ToString()), Times.Once);

        await harness.Stop();
    }

    [Test]
    public async Task GivenValidPlaceOrderCommand_WhenRepositoryThrowsException_ThenShouldPublishFailedEvent()
    {
        // Arrange
        _repositoryMock.Setup(x => x.DeleteBasketAsync(_basketId.ToString())).ReturnsAsync(false);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<PlaceOrderCommandHandler>())
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(_command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<PlaceOrderCommandHandler>();

        (await consumerHarness.Consumed.Any<PlaceOrderCommand>()).ShouldBeTrue();
        (await harness.Published.Any<BasketDeletedFailedIntegrationEvent>()).ShouldBeTrue();
        (await harness.Published.Any<BasketDeletedCompleteIntegrationEvent>()).ShouldBeFalse();

        _repositoryMock.Verify(x => x.DeleteBasketAsync(_basketId.ToString()), Times.Once);

        await harness.Stop();
    }

    [Test]
    public async Task GivenPlaceOrderCommand_ThenShouldVerifyInputContract()
    {
        // Verify the input command structure to ensure consumer contract compatibility
        await VerifySnapshot(
            new
            {
                CommandType = nameof(PlaceOrderCommand),
                Properties = new
                {
                    _command.BasketId,
                    _command.FullName,
                    _command.Email,
                    _command.OrderId,
                    _command.TotalMoney,
                },
                Schema = new
                {
                    BasketIdType = _command.BasketId.GetType().Name,
                    FullNameType = _command.FullName?.GetType().Name,
                    EmailType = _command.Email?.GetType().Name,
                    OrderIdType = _command.OrderId.GetType().Name,
                    TotalMoneyType = _command.TotalMoney.GetType().Name,
                    HasId = _command.Id != Guid.Empty,
                    HasCreationDate = _command.CreationDate != default,
                    IsIntegrationEvent = true,
                },
            }
        );
    }
}
