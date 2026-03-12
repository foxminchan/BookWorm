using BookWorm.Basket.Domain;
using BookWorm.Basket.IntegrationEvents.EventHandlers;
using BookWorm.Common;
using BookWorm.Contracts;
using Wolverine;

namespace BookWorm.Basket.ContractTests.Consumers;

public sealed class PlaceOrderConsumerTests
{
    private Guid _basketId;
    private PlaceOrderCommand _command = null!;
    private Mock<IBasketRepository> _repositoryMock = null!;
    private Mock<IMessageContext> _contextMock = null!;
    private List<object> _published = null!;
    private PlaceOrderCommandHandler _handler = null!;

    [Before(Test)]
    public void SetUp()
    {
        var orderId = Guid.CreateVersion7();
        _basketId = Guid.CreateVersion7();
        _repositoryMock = new();
        _published = [];
        _command = new(_basketId, "Test User", "test@example.com", orderId, 99.99m);

        _contextMock = new();
        _contextMock
            .Setup(x => x.PublishAsync(It.IsAny<object>(), null))
            .Callback<object, DeliveryOptions?>((msg, _) => _published.Add(msg))
            .Returns(ValueTask.CompletedTask);

        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenPlaceOrderCommand_WhenPublished_ThenConsumerShouldConsumeIt()
    {
        // Arrange
        _repositoryMock.Setup(x => x.DeleteBasketAsync(_basketId.ToString())).ReturnsAsync(true);

        // Act
        await _handler.Handle(_command, _contextMock.Object, CancellationToken.None);

        // Assert
        _published.OfType<BasketDeletedCompleteIntegrationEvent>().ShouldHaveSingleItem();

        await SnapshotTestHelper.Verify(new { command = _command, published = _published });

        _repositoryMock.Verify(x => x.DeleteBasketAsync(_basketId.ToString()), Times.Once);
    }
}
