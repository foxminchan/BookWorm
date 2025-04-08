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
}
