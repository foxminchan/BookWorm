using BookWorm.Basket.Domain;
using BookWorm.Basket.IntegrationEvents.EventHandlers;
using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Basket.ContractTests.Consumers;

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
    public async Task GivenPlaceOrderCommand_WhenPublished_ThenConsumerShouldConsumeIt()
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

        var consumedMessage = consumerHarness.Consumed.Select<PlaceOrderCommand>().First();

        // Verify the input command structure to ensure consumer contract compatibility
        await VerifySnapshot(
            new
            {
                CommandType = nameof(PlaceOrderCommand),
                Properties = new
                {
                    consumedMessage.Context.Message.BasketId,
                    consumedMessage.Context.Message.FullName,
                    consumedMessage.Context.Message.Email,
                    consumedMessage.Context.Message.OrderId,
                    consumedMessage.Context.Message.TotalMoney,
                },
                Schema = new
                {
                    BasketIdType = consumedMessage.Context.Message.BasketId.GetType().Name,
                    FullNameType = consumedMessage.Context.Message.FullName?.GetType().Name,
                    EmailType = consumedMessage.Context.Message.Email?.GetType().Name,
                    OrderIdType = consumedMessage.Context.Message.OrderId.GetType().Name,
                    TotalMoneyType = consumedMessage.Context.Message.TotalMoney.GetType().Name,
                    HasId = consumedMessage.Context.Message.Id != Guid.Empty,
                    HasCreationDate = consumedMessage.Context.Message.CreationDate != default,
                    IsIntegrationEvent = true,
                },
            }
        );

        _repositoryMock.Verify(x => x.DeleteBasketAsync(_basketId.ToString()), Times.Once);

        await harness.Stop();
    }
}
