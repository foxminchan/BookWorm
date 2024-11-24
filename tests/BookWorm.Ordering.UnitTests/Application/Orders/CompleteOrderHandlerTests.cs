using BookWorm.Contracts;
using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Ordering.Features.Orders.Complete;
using BookWorm.Ordering.UnitTests.Builder;

namespace BookWorm.Ordering.UnitTests.Application.Orders;

public sealed class CompleteOrderHandlerTests
{
    private readonly CompleteOrderHandler _handler;
    private readonly Mock<IIdentityService> _identityService = new();
    private readonly Mock<IPublishEndpoint> _publishEndpoint = new();
    private readonly Mock<IRepository<Order>> _repository = new();

    public CompleteOrderHandlerTests()
    {
        _handler = new(_repository.Object, _publishEndpoint.Object, _identityService.Object);
    }

    [Fact]
    public async Task GivenValidOrderId_ShouldCompleteOrderAndPublishEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = OrderBuilder.WithDefaultValues()[0];
        _repository
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(new(orderId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(Status.Completed);
        _repository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpoint.Verify(
            x =>
                x.Publish(
                    It.IsAny<OrderCompletedIntegrationEvent>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task GivenInvalidOrderId_ShouldThrowNotFoundException_WhenOrderNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _repository
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(new(orderId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _repository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _publishEndpoint.Verify(
            x =>
                x.Publish(
                    It.IsAny<OrderCompletedIntegrationEvent>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }
}
