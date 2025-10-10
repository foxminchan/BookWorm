using BookWorm.Catalog.Features.Publishers.Delete;
using BookWorm.Chassis.Endpoints;
using BookWorm.Chassis.Exceptions;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Publishers.Delete;

public sealed class DeletePublisherEndpointTests
{
    private readonly DeletePublisherEndpoint _endpoint = new();
    private readonly Guid _publisherId = Guid.CreateVersion7();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidPublisherId_WhenHandlingDeletePublisher_ThenShouldSendCommandAndReturnNoContent()
    {
        // Arrange
        _senderMock
            .Setup(s =>
                s.Send(
                    It.Is<DeletePublisherCommand>(c => c.Id == _publisherId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(_publisherId, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeletePublisherCommand>(c => c.Id == _publisherId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidPublisherId_WhenHandlingDeletePublisher_ThenShouldPassCorrectParametersToHandler()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        _senderMock
            .Setup(s => s.Send(It.IsAny<DeletePublisherCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        await _endpoint.HandleAsync(_publisherId, _senderMock.Object, cancellationToken);

        // Assert
        _senderMock.Verify(
            s =>
                s.Send(It.Is<DeletePublisherCommand>(c => c.Id == _publisherId), cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNonExistentPublisherId_WhenHandlingDeletePublisher_ThenShouldPropagateNotFoundException()
    {
        // Arrange
        var expectedException = new NotFoundException(
            $"Publisher with id {_publisherId} not found."
        );

        _senderMock
            .Setup(s => s.Send(It.IsAny<DeletePublisherCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _endpoint.HandleAsync(_publisherId, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe(expectedException.Message);

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeletePublisherCommand>(c => c.Id == _publisherId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenHandlerThrowsInvalidOperationException_WhenHandlingDeletePublisher_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException(
            "Cannot delete publisher with associated books."
        );

        _senderMock
            .Setup(s => s.Send(It.IsAny<DeletePublisherCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _endpoint.HandleAsync(_publisherId, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe(expectedException.Message);

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeletePublisherCommand>(c => c.Id == _publisherId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenHandlerThrowsGenericException_WhenHandlingDeletePublisher_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Unexpected error occurred.");

        _senderMock
            .Setup(s => s.Send(It.IsAny<DeletePublisherCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _endpoint.HandleAsync(_publisherId, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<Exception>();
        exception.Message.ShouldBe(expectedException.Message);
    }

    [Test]
    public async Task GivenEmptyGuid_WhenHandlingDeletePublisher_ThenShouldStillCallSenderWithEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        _senderMock
            .Setup(s => s.Send(It.IsAny<DeletePublisherCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(emptyGuid, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeletePublisherCommand>(c => c.Id == emptyGuid),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCancellationTokenRequested_WhenHandlingDeletePublisher_ThenShouldPassCancellationTokenToSender()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _senderMock
            .Setup(s => s.Send(It.IsAny<DeletePublisherCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        await _endpoint.HandleAsync(_publisherId, _senderMock.Object, cancellationToken);

        // Assert
        _senderMock.Verify(
            s =>
                s.Send(It.Is<DeletePublisherCommand>(c => c.Id == _publisherId), cancellationToken),
            Times.Once
        );
    }

    [Test]
    public void GivenEndpointInstance_WhenCheckingImplementation_ThenShouldImplementIEndpointInterface()
    {
        // Arrange & Act & Assert
        _endpoint.ShouldNotBeNull();
        _endpoint.ShouldBeAssignableTo<IEndpoint<NoContent, Guid, ISender>>();
    }

    [Test]
    public async Task GivenValidRequest_WhenHandlingDeletePublisher_ThenShouldReturnNoContentResult()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeletePublisherCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(_publisherId, _senderMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<NoContent>();
    }

    [Test]
    public async Task GivenMultipleCallsWithSameId_WhenHandlingDeletePublisher_ThenShouldCallSenderMultipleTimes()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeletePublisherCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        await _endpoint.HandleAsync(_publisherId, _senderMock.Object);
        await _endpoint.HandleAsync(_publisherId, _senderMock.Object);

        // Assert
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeletePublisherCommand>(c => c.Id == _publisherId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Exactly(2)
        );
    }
}
