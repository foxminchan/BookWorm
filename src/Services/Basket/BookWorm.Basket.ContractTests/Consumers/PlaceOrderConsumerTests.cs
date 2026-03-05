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
    private Guid _basketId;
    private PlaceOrderCommand _command = null!;
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;
    private Mock<IBasketRepository> _repositoryMock = null!;

    [Before(Test)]
    public async Task SetUpAsync()
    {
        var orderId = Guid.CreateVersion7();
        _basketId = Guid.CreateVersion7();
        _repositoryMock = new();
        _command = new(_basketId, "Test User", "test@example.com", orderId, 99.99m);

        _provider = new ServiceCollection()
            .AddTelemetryListener()
            .AddMassTransitTestHarness(x => x.AddConsumer<PlaceOrderCommandHandler>())
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
    public async Task GivenPlaceOrderCommand_WhenPublished_ThenConsumerShouldConsumeIt()
    {
        // Arrange
        _repositoryMock.Setup(x => x.DeleteBasketAsync(_basketId.ToString())).ReturnsAsync(true);

        // Act
        await _harness.Bus.Publish(_command);

        // Assert
        var consumer = _harness.GetConsumerHarness<PlaceOrderCommandHandler>();
        await consumer.Consumed.Any<PlaceOrderCommand>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

        _repositoryMock.Verify(x => x.DeleteBasketAsync(_basketId.ToString()), Times.Once);
    }
}
