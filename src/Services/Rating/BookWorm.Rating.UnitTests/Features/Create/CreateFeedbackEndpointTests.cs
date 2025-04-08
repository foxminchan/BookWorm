using BookWorm.Rating.Features.Create;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Rating.UnitTests.Features.Create;

public sealed class CreateFeedbackEndpointTests
{
    private readonly CreateFeedbackEndpoint _endpoint;
    private readonly Guid _resultId;
    private readonly Mock<ISender> _senderMock;
    private readonly CreateFeedbackCommand _validCommand;

    public CreateFeedbackEndpointTests()
    {
        _senderMock = new();
        _endpoint = new();
        var testBookId = Guid.CreateVersion7();
        _resultId = Guid.CreateVersion7();

        _validCommand = new(testBookId, "John", "Doe", "Great book!", 5);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandleAsync_ThenShouldReturnCreatedWithCorrectId()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(_validCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_resultId);

        // Act
        var result = await _endpoint.HandleAsync(_validCommand, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<Guid>>();
        result.Value.ShouldBe(_resultId);
        _senderMock.Verify(x => x.Send(_validCommand, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenCommand_WhenHandleAsync_ThenShouldPassCancellationToken()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        _senderMock.Setup(x => x.Send(_validCommand, cancellationToken)).ReturnsAsync(_resultId);

        // Act
        var result = await _endpoint.HandleAsync(
            _validCommand,
            _senderMock.Object,
            cancellationToken
        );

        // Assert
        result.ShouldNotBeNull();
        _senderMock.Verify(x => x.Send(_validCommand, cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandleAsync_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");

        _senderMock
            .Setup(x => x.Send(_validCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _endpoint.HandleAsync(_validCommand, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Test exception");
    }
}
