using BookWorm.Basket.Domain;
using BookWorm.Basket.IntegrationEvents.EventHandlers;
using BookWorm.Common;
using BookWorm.Contracts;
using Wolverine;

namespace BookWorm.Basket.ContractTests.Publishers;

public sealed class BasketDeletedEventPublisherTests
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
    public async Task GivenBasketDeleteSucceeds_WhenHandlingPlaceOrder_ThenShouldPublishCompleteEvent()
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

    [Test]
    public async Task GivenBasketDeleteFails_WhenHandlingPlaceOrder_ThenShouldPublishFailedEvent()
    {
        // Arrange
        _repositoryMock.Setup(x => x.DeleteBasketAsync(_basketId.ToString())).ReturnsAsync(false);

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
