using BookWorm.Catalog.Features.Books.Delete;
using BookWorm.Chassis.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Books.Delete;

public sealed class DeleteBookEndpointTests
{
    private readonly Guid _bookId = Guid.CreateVersion7();
    private readonly DeleteBookEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidBookId_WhenHandlingDeleteEndpoint_ThenShouldSendCommandAndReturnNoContent()
    {
        // Arrange
        _senderMock
            .Setup(s =>
                s.Send(
                    It.Is<DeleteBookCommand>(c => c.Id == _bookId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(_bookId, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteBookCommand>(c => c.Id == _bookId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenDeleteBookCommand_WhenHandleFails_ThenExceptionShouldPropagate()
    {
        // Arrange
        var expectedException = new NotFoundException($"Book with id {_bookId} not found.");
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteBookCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _endpoint.HandleAsync(_bookId, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe(expectedException.Message);
    }
}
