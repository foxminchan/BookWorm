using BookWorm.Catalog.Features.Publishers.Create;
using MediatR;

namespace BookWorm.Catalog.UnitTests.Features.Publishers.Create;

public sealed class CreatePublisherEndpointTests
{
    private readonly CreatePublisherEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();
    private readonly CreatePublisherCommand _validCommand = new("Test Publisher");

    [Test]
    public async Task GivenExceptionFromSender_WhenHandlingCreatePublisher_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        _senderMock
            .Setup(s => s.Send(_validCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _endpoint.HandleAsync(_validCommand, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe(expectedException.Message);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreatePublisher_ThenShouldReturnOkWithPublisherId()
    {
        // Arrange
        var expectedPublisherId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(_validCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPublisherId);

        // Act
        var result = await _endpoint.HandleAsync(_validCommand, _senderMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(expectedPublisherId);
        _senderMock.Verify(s => s.Send(_validCommand, It.IsAny<CancellationToken>()), Times.Once);
    }
}
