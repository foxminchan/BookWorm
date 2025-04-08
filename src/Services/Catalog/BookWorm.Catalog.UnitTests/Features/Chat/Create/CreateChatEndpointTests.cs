using BookWorm.Catalog.Features.Chat;
using BookWorm.Catalog.Features.Chat.Create;
using BookWorm.Catalog.Infrastructure.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Chat.Create;

public sealed class CreateChatEndpointTests
{
    private readonly Mock<IChatStreaming> _chatStreamingMock;
    private readonly CreateChatEndpoint _endpoint;
    private readonly Guid _expectedGuid;
    private readonly Prompt _prompt;

    public CreateChatEndpointTests()
    {
        _prompt = new("Test prompt text");
        _expectedGuid = Guid.CreateVersion7();

        _chatStreamingMock = new();
        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(_prompt.Text))
            .ReturnsAsync(_expectedGuid);

        _endpoint = new();
    }

    [Test]
    public async Task GivenValidPrompt_WhenHandlingCreateChat_ThenShouldReturnCreatedWithCorrectGuid()
    {
        // Act
        var result = await _endpoint.HandleAsync(
            _prompt,
            _chatStreamingMock.Object,
            CancellationToken.None
        );

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Ok<Guid>>();
        result.Value.ShouldBe(_expectedGuid);

        // Verify chat streaming was called with correct prompt
        _chatStreamingMock.Verify(x => x.AddStreamingMessage(_prompt.Text), Times.Once);
    }
}
