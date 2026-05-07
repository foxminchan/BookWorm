using BookWorm.Basket.Domain;
using BookWorm.Basket.IntegrationEvents.EventHandlers;
using BookWorm.Common;
using BookWorm.Contracts;
using Wolverine;

namespace BookWorm.Basket.ContractTests.Consumers;

public sealed class PlaceOrderConsumerTests
{
    private const string Email = "test@example.com";
    private const decimal TotalMoney = 99.99m;
    private Guid _basketId;
    private Guid _orderId;
    private Mock<IBasketRepository> _repositoryMock = null!;

    [Before(Test)]
    public void SetUp()
    {
        _basketId = Guid.CreateVersion7();
        _orderId = Guid.CreateVersion7();
        _repositoryMock = new();
    }

    [Test]
    public async Task GivenValidPlaceOrderCommand_WhenHandling_ThenShouldDeleteBasket()
    {
        // Arrange
        _repositoryMock.Setup(x => x.DeleteBasketAsync(_basketId.ToString())).ReturnsAsync(true);

        var command = new PlaceOrderCommand(_basketId, "Test User", Email, _orderId, TotalMoney);
        var bus = new TestMessageContext();
        var handler = new PlaceOrderCommandHandler(_repositoryMock.Object, bus);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(
            bus.AllOutgoing.Select(e => ((Envelope)e).Message).ToList()
        );
        _repositoryMock.Verify(x => x.DeleteBasketAsync(_basketId.ToString()), Times.Once);
    }
}
