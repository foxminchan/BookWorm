using BookWorm.Ordering.Features.Orders.Create;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BookWorm.Ordering.UnitTests.Features.Orders.Create;

public sealed class CreateOrderEndpointTests
{
    private readonly CreateOrderEndpoint _endpoint = new();
    private readonly LinkGenerator _linkGenerator = Mock.Of<LinkGenerator>();
    private readonly Guid _orderId = Guid.CreateVersion7();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreateOrder_ThenShouldReturnCreatedResult()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_orderId);

        // Act
        var result = await _endpoint.HandleAsync(
            _senderMock.Object,
            _linkGenerator,
            CancellationToken.None
        );

        // Assert
        result.ShouldBeOfType<Created<Guid>>();
        result.Value.ShouldBe(_orderId);

        // Verify the location header contains the correct URL
        result.Location?.ShouldContain($"Orders/{_orderId}");

        // Verify the command was sent
        _senderMock.Verify(
            x => x.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandlingCreateOrder_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");

        _senderMock
            .Setup(x => x.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () =>
            await _endpoint.HandleAsync(_senderMock.Object, _linkGenerator, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Test exception");
    }
}
