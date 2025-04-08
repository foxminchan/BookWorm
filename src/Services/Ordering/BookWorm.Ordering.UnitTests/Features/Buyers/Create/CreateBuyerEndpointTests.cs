using BookWorm.Ordering.Features.Buyers.Create;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BookWorm.Ordering.UnitTests.Features.Buyers.Create;

public sealed class CreateBuyerEndpointTests
{
    private readonly CreateBuyerEndpoint _endpoint = new();
    private readonly Mock<LinkGenerator> _linkGeneratorMock = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidCommand_WhenHandlingRequest_ThenShouldReturnCreatedResponse()
    {
        // Arrange
        var command = new CreateBuyerCommand("123 Main St", "Seattle", "WA");
        var buyerId = Guid.CreateVersion7();

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(buyerId);

        // Act
        var result = await _endpoint.HandleAsync(
            command,
            _senderMock.Object,
            _linkGeneratorMock.Object
        );

        // Assert
        result.ShouldBeOfType<Created<Guid>>();
        result.Value.ShouldBe(buyerId);
        _senderMock.Verify(s => s.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}
