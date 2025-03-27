using BookWorm.Basket.Domain;
using BookWorm.Basket.IntegrationEvents.EventHandlers;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Basket.UnitTests.Consumers;

public sealed class PlaceOrderConsumerTests
{
    private readonly Guid _basketId;
    private readonly PlaceOrderCommand _command;
    private readonly string _email;
    private readonly Guid _orderId;
    private readonly Mock<IBasketRepository> _repositoryMock;
    private readonly decimal _totalMoney;

    public PlaceOrderConsumerTests()
    {
        _orderId = Guid.CreateVersion7();
        _basketId = Guid.CreateVersion7();
        _email = "test@example.com";
        _totalMoney = 99.99m;
        _repositoryMock = new();
        _command = new(_basketId, _email, _orderId, _totalMoney);
    }

    [Test]
    public async Task GivenValidPlaceOrderCommand_WhenBasketDeleteSucceeds_ThenShouldPublishCompleteEvent()
    {
        // Arrange
        _repositoryMock.Setup(x => x.DeleteBasketAsync(_basketId.ToString())).ReturnsAsync(true);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<PlaceOrderCommandHandler>())
            .AddScoped<IBasketRepository>(_ => _repositoryMock.Object)
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

        publishedMessage.Context.Message.OrderId.ShouldBe(_orderId);
        publishedMessage.Context.Message.BasketId.ShouldBe(_basketId);
        publishedMessage.Context.Message.TotalMoney.ShouldBe(_totalMoney);

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
            .AddScoped<IBasketRepository>(_ => _repositoryMock.Object)
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

        publishedMessage.Context.Message.OrderId.ShouldBe(_orderId);
        publishedMessage.Context.Message.BasketId.ShouldBe(_basketId);
        publishedMessage.Context.Message.Email.ShouldBe(_email);
        publishedMessage.Context.Message.TotalMoney.ShouldBe(_totalMoney);

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
            .AddScoped<IBasketRepository>(_ => _repositoryMock.Object)
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
}
