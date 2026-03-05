using BookWorm.Basket.Features.Create;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BookWorm.Basket.UnitTests.Features.Create;

public sealed class CreateBasketEndpointTests
{
    private CreateBasketCommand _command = null!;
    private CreateBasketEndpoint _endpoint = null!;
    private string _expectedBasketId = null!;
    private LinkGenerator _linkGenerator = null!;
    private Mock<ISender> _senderMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _command = new([new("book1", 1), new("book2", 2)]);
        _endpoint = new();
        _expectedBasketId = Guid.CreateVersion7().ToString();
        _linkGenerator = Mock.Of<LinkGenerator>();
        _senderMock = new();
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingRequest_ThenShouldCallMediator()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(_command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedBasketId);

        // Act
        await _endpoint.HandleAsync(_command, _senderMock.Object, _linkGenerator);

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
        var result = await _endpoint.HandleAsync(_command, _senderMock.Object, _linkGenerator);

        // Assert
        result.ShouldBeOfType<Created<string>>();
        result.Value.ShouldBe(_expectedBasketId);
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
        var act = () => _endpoint.HandleAsync(_command, _senderMock.Object, _linkGenerator);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Test exception");
    }
}
