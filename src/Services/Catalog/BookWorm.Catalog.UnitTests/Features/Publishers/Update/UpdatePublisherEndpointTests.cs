using BookWorm.Catalog.Features.Publishers.Update;
using BookWorm.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Publishers.Update;

public sealed class UpdatePublisherEndpointTests
{
    private const string PublisherName = "Updated Publisher Name";
    private readonly UpdatePublisherEndpoint _endpoint = new();
    private readonly Guid _publisherId = Guid.CreateVersion7();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidCommand_WhenHandlingUpdatePublisher_ThenShouldSendCommandAndReturnNoContent()
    {
        // Arrange
        var command = new UpdatePublisherCommand(_publisherId, PublisherName);
        _senderMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(command, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenNonExistingPublisher_WhenHandlingUpdatePublisher_ThenShouldPropagateNotFoundException()
    {
        // Arrange
        var command = new UpdatePublisherCommand(_publisherId, PublisherName);
        var exception = new NotFoundException($"Publisher with id {_publisherId} not found.");

        _senderMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var act = async () => await _endpoint.HandleAsync(command, _senderMock.Object);

        // Assert
        var thrownException = await act.ShouldThrowAsync<NotFoundException>();
        thrownException.Message.ShouldBe($"Publisher with id {_publisherId} not found.");
        _senderMock.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenValidationException_WhenHandlingUpdatePublisher_ThenShouldPropagateValidationException()
    {
        // Arrange
        var command = new UpdatePublisherCommand(_publisherId, string.Empty);
        var exception = new ValidationException("Validation failed");

        _senderMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var act = async () => await _endpoint.HandleAsync(command, _senderMock.Object);

        // Assert
        var thrownException = await act.ShouldThrowAsync<ValidationException>();
        thrownException.Message.ShouldBe("Validation failed");
        _senderMock.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}
