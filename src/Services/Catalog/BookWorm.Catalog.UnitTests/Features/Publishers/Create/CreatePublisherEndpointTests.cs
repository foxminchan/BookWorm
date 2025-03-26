using BookWorm.Catalog.Features.Publishers.Create;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Publishers.Create;

public sealed class CreatePublisherEndpointTests
{
    private readonly CreatePublisherEndpoint _endpoint = new();
    private readonly Guid _publisherId = Guid.CreateVersion7();
    private readonly Mock<ISender> _senderMock = new();
    private readonly CreatePublisherCommand _validCommand = new("Test Publisher");

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreatePublisher_ThenShouldReturnCreatedResult()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(_validCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_publisherId);

        // Act
        var result = await _endpoint.HandleAsync(_validCommand, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Created<Guid>>();
        result.Value.ShouldBe(_publisherId);
        result.Location.ShouldBe($"/api/1/publishers/{_publisherId}");
        _senderMock.Verify(s => s.Send(_validCommand, It.IsAny<CancellationToken>()), Times.Once);
    }

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
