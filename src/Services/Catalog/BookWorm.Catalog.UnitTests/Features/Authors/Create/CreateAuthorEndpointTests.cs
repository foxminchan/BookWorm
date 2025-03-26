using BookWorm.Catalog.Features.Authors.Create;
using BookWorm.SharedKernel.SeedWork;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Authors.Create;

public sealed class CreateAuthorEndpointTests
{
    private readonly Guid _authorId = Guid.CreateVersion7();
    private readonly CreateAuthorCommand _command = new("Test Author");
    private readonly CreateAuthorEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidCommand_WhenHandleAsyncCalled_ThenShouldCallSenderWithCommand()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(_command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_authorId);

        // Act
        await _endpoint.HandleAsync(_command, _senderMock.Object);

        // Assert
        _senderMock.Verify(x => x.Send(_command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandleAsyncCalled_ThenShouldReturnCreatedWithCorrectUrl()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(_command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_authorId);

        var expectedUrl = new UrlBuilder()
            .WithVersion()
            .WithResource(nameof(Authors))
            .WithId(_authorId)
            .Build();

        // Act
        var result = await _endpoint.HandleAsync(_command, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Created<Guid>>();
        result.Location.ShouldBe(expectedUrl);
        result.Value.ShouldBe(_authorId);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandleAsyncCalled_ThenShouldUseCorrectUrlFormat()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(_command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_authorId);

        // Act
        var result = await _endpoint.HandleAsync(_command, _senderMock.Object);

        // Assert
        result.Location.ShouldBe($"/api/1/authors/{_authorId}");
    }

    [Test]
    public async Task GivenCancellationRequested_WhenHandleAsyncCalled_ThenShouldPassCancellationToken()
    {
        // Arrange
        var cancellationToken = new CancellationToken(true);

        _senderMock.Setup(x => x.Send(_command, cancellationToken)).ReturnsAsync(_authorId);

        // Act & Assert
        await _endpoint.HandleAsync(_command, _senderMock.Object, cancellationToken);

        _senderMock.Verify(x => x.Send(_command, cancellationToken), Times.Once);
    }
}
