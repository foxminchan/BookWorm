using BookWorm.Catalog.Features.Chat.Cancel;
using BookWorm.Catalog.Infrastructure.GenAi.CancellationManager;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Chat.Cancel;

public sealed class CancelChatEndpointTests
{
    private readonly Mock<ICancellationManager> _cancellationManagerMock = new();
    private readonly Guid _chatId = Guid.CreateVersion7();
    private readonly CancelChatEndpoint _endpoint = new();

    [Test]
    public async Task GivenValidChatId_WhenHandlingCancelChat_ThenShouldCallCancelAsync()
    {
        // Act
        var result = await _endpoint.HandleAsync(_chatId, _cancellationManagerMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _cancellationManagerMock.Verify(m => m.CancelAsync(_chatId), Times.Once);
    }

    [Test]
    public async Task GivenCancellationManagerThrowsException_WhenHandlingCancelChat_ThenShouldPropagateException()
    {
        // Arrange
        _cancellationManagerMock
            .Setup(m => m.CancelAsync(_chatId))
            .ThrowsAsync(new InvalidOperationException("Cancellation failed"));

        // Act
        var act = async () => await _endpoint.HandleAsync(_chatId, _cancellationManagerMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Cancellation failed");
    }
}
