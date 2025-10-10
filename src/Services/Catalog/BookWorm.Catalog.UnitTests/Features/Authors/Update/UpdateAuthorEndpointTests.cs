using BookWorm.Catalog.Features.Authors.Update;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Authors.Update;

public sealed class UpdateAuthorEndpointTests
{
    private readonly string _authorName;
    private readonly UpdateAuthorCommand _command;
    private readonly UpdateAuthorEndpoint _endpoint;
    private readonly Mock<ISender> _senderMock;

    public UpdateAuthorEndpointTests()
    {
        _senderMock = new();
        _endpoint = new();
        var authorId = Guid.CreateVersion7();
        _authorName = "Updated Author Name";
        _command = new(authorId, _authorName);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingUpdateAuthor_ThenShouldCallSendAndReturnNoContent()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateAuthorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(
            _command,
            _senderMock.Object,
            CancellationToken.None
        );

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(s => s.Send(_command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenCommandWithDifferentId_WhenHandlingUpdateAuthor_ThenShouldUpdateCommandId()
    {
        // Arrange
        var originalId = Guid.CreateVersion7();
        var updatedId = Guid.CreateVersion7();
        var commandWithDifferentId = new UpdateAuthorCommand(originalId, _authorName);

        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateAuthorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        await _endpoint.HandleAsync(
            commandWithDifferentId with
            {
                Id = updatedId,
            },
            _senderMock.Object
        );

        // Assert
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<UpdateAuthorCommand>(c => c.Id == updatedId && c.Name == _authorName),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
