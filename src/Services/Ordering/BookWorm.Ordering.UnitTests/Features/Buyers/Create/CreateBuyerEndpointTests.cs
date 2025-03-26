using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;
using BookWorm.Ordering.Features.Buyers.Create;
using BookWorm.SharedKernel.SeedWork;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.UnitTests.Features.Buyers.Create;

public sealed class CreateBuyerEndpointTests
{
    private readonly CreateBuyerEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidCommand_WhenHandlingRequest_ThenShouldReturnCreatedResponse()
    {
        // Arrange
        var command = new CreateBuyerCommand("123 Main St", "Seattle", "WA");
        var buyerId = Guid.NewGuid();

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(buyerId);

        // Act
        var result = await _endpoint.HandleAsync(command, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Created<Guid>>();
        result.Value.ShouldBe(buyerId);
        result.Location.ShouldBe($"/api/buyers/{buyerId}");
        _senderMock.Verify(s => s.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void GivenValidCommand_WhenBuildingUrl_ThenShouldConstructCorrectUrl()
    {
        // Arrange
        var buyerId = Guid.NewGuid();

        // Act
        var url = new UrlBuilder().WithResource(nameof(Buyer)).WithId(buyerId).Build();

        // Assert
        url.ShouldBe($"/api/buyers/{buyerId}");
    }
}
