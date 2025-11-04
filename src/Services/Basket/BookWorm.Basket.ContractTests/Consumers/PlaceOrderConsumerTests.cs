using BookWorm.Basket.Domain;
using BookWorm.Basket.IntegrationEvents.EventHandlers;
using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Basket.ContractTests.Consumers;

public sealed class PlaceOrderConsumerTests
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

        try
        {
            // Act
            await harness.Bus.Publish(_command);

            // Assert
            var consumer = harness.GetConsumerHarness<PlaceOrderCommandHandler>();

            await SnapshotTestHelper.Verify(new { harness, consumer });

            _repositoryMock.Verify(x => x.DeleteBasketAsync(_basketId.ToString()), Times.Once);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
