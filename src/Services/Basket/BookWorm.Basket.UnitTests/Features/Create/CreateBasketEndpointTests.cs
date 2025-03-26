using BookWorm.Basket.Features;
using BookWorm.Basket.Features.Create;
using BookWorm.SharedKernel.SeedWork;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Basket.UnitTests.Features.Create;

public sealed class CreateBasketEndpointTests
{
    private readonly CreateBasketCommand _command = new(
        [new BasketItemRequest("book1", 1), new BasketItemRequest("book2", 2)]
    );

    private readonly CreateBasketEndpoint _endpoint = new();

    private readonly string _expectedBasketId = Guid.CreateVersion7().ToString();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidCommand_WhenHandlingRequest_ThenShouldCallMediator()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(_command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedBasketId);

        // Act
        await _endpoint.HandleAsync(_command, _senderMock.Object);

        // Assert
        _senderMock.Verify(x => x.Send(_command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingRequest_ThenShouldReturnCreatedResult()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(_command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedBasketId);

        // Act
        var result = await _endpoint.HandleAsync(_command, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Created<string>>();
        result.Value.ShouldBe(_expectedBasketId);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingRequest_ThenShouldReturnCorrectLocationHeader()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(_command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedBasketId);

        var expectedUrl = new UrlBuilder()
            .WithResource(nameof(Basket))
            .WithId(_expectedBasketId)
            .Build();

        // Act
        var result = await _endpoint.HandleAsync(_command, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Created<string>>();
        result.Location.ShouldBe(expectedUrl);
    }

    [Test]
    public async Task GivenMediatrThrowsException_WhenHandlingRequest_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        _senderMock
            .Setup(x => x.Send(_command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = () => _endpoint.HandleAsync(_command, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Test exception");
    }
}
