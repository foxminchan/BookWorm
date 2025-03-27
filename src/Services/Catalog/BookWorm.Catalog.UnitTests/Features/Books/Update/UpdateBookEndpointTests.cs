using BookWorm.Catalog.Features.Books.Update;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Books.Update;

public sealed class UpdateBookEndpointTests
{
    private readonly UpdateBookCommand _command;
    private readonly UpdateBookEndpoint _endpoint;
    private readonly Mock<ISender> _senderMock;

    public UpdateBookEndpointTests()
    {
        var bookId = Guid.CreateVersion7();
        _senderMock = new();
        _endpoint = new();

        // Create a sample command with required properties
        _command = new(
            bookId,
            "Updated Book Title",
            "Updated book description",
            null,
            29.99m,
            24.99m,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7(), Guid.CreateVersion7()]
        );
    }

    [Test]
    public async Task GivenValidCommand_WhenHandleAsync_ThenShouldSendCommandAndReturnNoContent()
    {
        // Arrange
        _senderMock.Setup(x => x.Send(_command, CancellationToken.None)).ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(
            _command,
            _senderMock.Object,
            CancellationToken.None
        );

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(x => x.Send(_command, CancellationToken.None), Times.Once);
    }
}
