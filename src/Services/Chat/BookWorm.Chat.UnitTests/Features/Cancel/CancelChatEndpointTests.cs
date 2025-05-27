using BookWorm.Chat.Features.Cancel;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Chat.UnitTests.Features.Cancel;

public sealed class CancelChatEndpointTests
{
    private readonly Guid _chatId = Guid.CreateVersion7();
    private readonly CancelChatEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidChatId_WhenHandlingCancelChat_ThenShouldCallCancelAsync()
    {
        // Act
        var result = await _endpoint.HandleAsync(_chatId, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(
            m => m.Send(new CancelChatCommand(_chatId), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCancellationManagerThrowsException_WhenHandlingCancelChat_ThenShouldPropagateException()
    {
        // Arrange
        _senderMock
            .Setup(m => m.Send(new CancelChatCommand(_chatId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cancellation failed"));

        // Act
        var act = async () => await _endpoint.HandleAsync(_chatId, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Cancellation failed");
    }
}
