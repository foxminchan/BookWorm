using BookWorm.Rating.Features.Delete;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Rating.UnitTests.Features.Delete;

public sealed class DeleteFeedbackEndpointTests
{
    private readonly DeleteFeedbackEndpoint _endpoint = new();
    private readonly Guid _feedbackId = Guid.CreateVersion7();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidId_WhenHandlingEndpointRequest_ThenShouldCallSenderWithCommand()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteFeedbackCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        await _endpoint.HandleAsync(_feedbackId, _senderMock.Object);

        // Assert
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteFeedbackCommand>(c => c.Id == _feedbackId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidRequest_WhenHandlingEndpoint_ThenShouldReturnNoContent()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteFeedbackCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(_feedbackId, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
    }
}
