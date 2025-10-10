using BookWorm.Catalog.Features.Authors.Delete;
using BookWorm.Chassis.Exceptions;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Authors.Delete;

public sealed class DeleteAuthorEndpointTests
{
    private readonly Guid _authorId = Guid.CreateVersion7();
    private readonly DeleteAuthorEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidAuthorId_WhenHandlingDeleteRequest_ThenShouldReturnNoContent()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteAuthorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(_authorId, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteAuthorCommand>(c => c.Id == _authorId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNonExistentAuthorId_WhenHandlingDeleteRequest_ThenShouldPropagateNotFoundException()
    {
        // Arrange
        var exceptionMessage = $"Author with id {_authorId} not found.";
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteAuthorCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(exceptionMessage));

        // Act
        var act = async () => await _endpoint.HandleAsync(_authorId, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe(exceptionMessage);
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteAuthorCommand>(c => c.Id == _authorId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
