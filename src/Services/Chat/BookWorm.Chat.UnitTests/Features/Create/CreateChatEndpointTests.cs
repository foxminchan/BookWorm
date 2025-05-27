using BookWorm.Chat.Features.Create;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BookWorm.Chat.UnitTests.Features.Create;

public sealed class CreateChatEndpointTests
{
    private readonly CreateChatCommand _command;
    private readonly CreateChatEndpoint _endpoint;
    private readonly Guid _expectedGuid;
    private readonly Mock<LinkGenerator> _linkGeneratorMock;
    private readonly Mock<ISender> _senderMock;

    public CreateChatEndpointTests()
    {
        _expectedGuid = Guid.CreateVersion7();
        _command = new("Test chat name");

        _senderMock = new();
        _senderMock
            .Setup(x => x.Send(It.IsAny<CreateChatCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedGuid);

        _linkGeneratorMock = new();
        _endpoint = new();
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreateChat_ThenShouldReturnCreatedWithCorrectGuid()
    {
        // Act
        var result = await _endpoint.HandleAsync(
            _command,
            _senderMock.Object,
            _linkGeneratorMock.Object,
            CancellationToken.None
        );

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Created<Guid>>();
        result.Value.ShouldBe(_expectedGuid);
        _senderMock.Verify(x => x.Send(_command, It.IsAny<CancellationToken>()), Times.Once);
    }
}
