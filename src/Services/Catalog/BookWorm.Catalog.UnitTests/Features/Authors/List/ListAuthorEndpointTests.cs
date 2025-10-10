using BookWorm.Catalog.Features.Authors;
using BookWorm.Catalog.Features.Authors.List;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Authors.List;

public sealed class ListAuthorEndpointTests
{
    private readonly ListAuthorsEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidQuery_WhenHandlingRequest_ThenShouldReturnOkResultWithAuthors()
    {
        // Arrange
        List<AuthorDto> authors =
        [
            new(Guid.CreateVersion7(), "Author 1"),
            new(Guid.CreateVersion7(), "Author 2"),
        ];

        _senderMock
            .Setup(sender =>
                sender.Send(It.IsAny<ListAuthorsQuery>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(authors);

        // Act
        var result = await _endpoint.HandleAsync(_senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<IReadOnlyList<AuthorDto>>>();
        result.Value.ShouldBeEquivalentTo(authors);

        _senderMock.Verify(
            sender => sender.Send(It.IsAny<ListAuthorsQuery>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandlingRequest_ThenShouldPropagateException()
    {
        // Arrange
        _senderMock
            .Setup(sender =>
                sender.Send(It.IsAny<ListAuthorsQuery>(), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new());

        // Act
        Func<Task> act = async () => await _endpoint.HandleAsync(_senderMock.Object);

        // Assert
        await act.ShouldThrowAsync<Exception>();

        _senderMock.Verify(
            sender => sender.Send(It.IsAny<ListAuthorsQuery>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
