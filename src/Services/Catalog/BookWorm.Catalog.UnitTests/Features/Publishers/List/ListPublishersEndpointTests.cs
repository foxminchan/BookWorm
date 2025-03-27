using BookWorm.Catalog.Features.Publishers;
using BookWorm.Catalog.Features.Publishers.List;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Publishers.List;

public sealed class ListPublishersEndpointTests
{
    private readonly ListPublishersEndpoint _endpoint = new();

    private readonly List<PublisherDto> _expectedPublishers =
    [
        new(Guid.CreateVersion7(), "Publisher 1"),
        new(Guid.CreateVersion7(), "Publisher 2"),
        new(Guid.CreateVersion7(), "Publisher 3"),
    ];

    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenSender_WhenHandlingRequest_ThenShouldSendListPublishersQuery()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(It.IsAny<ListPublishersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedPublishers);

        // Act
        await _endpoint.HandleAsync(_senderMock.Object);

        // Assert
        _senderMock.Verify(
            x => x.Send(It.IsAny<ListPublishersQuery>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSender_WhenHandlingRequest_ThenShouldReturnOkResultWithPublishers()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(It.IsAny<ListPublishersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedPublishers);

        // Act
        var result = await _endpoint.HandleAsync(_senderMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Ok<IReadOnlyList<PublisherDto>>>();
        result.Value.ShouldBe(_expectedPublishers);
    }

    [Test]
    public async Task GivenSenderAndCancellationToken_WhenHandlingRequest_ThenShouldPassTokenToSender()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        _senderMock
            .Setup(x => x.Send(It.IsAny<ListPublishersQuery>(), cancellationToken))
            .ReturnsAsync(_expectedPublishers);

        // Act
        await _endpoint.HandleAsync(_senderMock.Object, cancellationToken);

        // Assert
        _senderMock.Verify(
            x => x.Send(It.IsAny<ListPublishersQuery>(), cancellationToken),
            Times.Once
        );
    }
}
