using BookWorm.Catalog.Features.Books.Create;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BookWorm.Catalog.UnitTests.Features.Books.Create;

public sealed class CreateBookEndpointTests
{
    private readonly CreateBookCommand _command;
    private readonly CreateBookEndpoint _endpoint;
    private readonly Guid _expectedId;
    private readonly LinkGenerator _linkGenerator;
    private readonly Mock<ISender> _senderMock;

    public CreateBookEndpointTests()
    {
        _senderMock = new();
        _endpoint = new();
        _expectedId = Guid.CreateVersion7();
        _linkGenerator = new Mock<LinkGenerator>().Object;

        // Create a mock IFormFile for the image
        var imageMock = new Mock<IFormFile>();

        // Create a valid command with test data
        _command = new(
            "Test Book",
            "Test Description",
            imageMock.Object,
            19.99m,
            null,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            [Guid.CreateVersion7()]
        );
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreateBook_ThenShouldCallSenderWithCommand()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateBookCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedId);

        // Act
        await _endpoint.HandleAsync(_command, _senderMock.Object, _linkGenerator);

        // Assert
        _senderMock.Verify(s => s.Send(_command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreateBook_ThenShouldReturnCreatedWithExpectedId()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateBookCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedId);

        // Act
        var result = await _endpoint.HandleAsync(_command, _senderMock.Object, _linkGenerator);

        // Assert
        result.ShouldBeOfType<Created<Guid>>();
        result.Value.ShouldBe(_expectedId);
    }

    [Test]
    public async Task GivenExceptionFromSender_WhenHandlingCreateBook_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateBookCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () =>
            await _endpoint.HandleAsync(_command, _senderMock.Object, _linkGenerator);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe(expectedException.Message);
    }
}
