using BookWorm.Catalog.Features.Categories.Update;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Categories.Update;

public sealed class UpdateCategoryEndpointTests
{
    private readonly UpdateCategoryCommand _command;
    private readonly UpdateCategoryEndpoint _endpoint;
    private readonly Mock<ISender> _senderMock;

    public UpdateCategoryEndpointTests()
    {
        _senderMock = new();
        _endpoint = new();
        var categoryId = Guid.CreateVersion7();
        _command = new(categoryId, "Updated Category Name");
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingUpdateCategory_ThenShouldReturnNoContent()
    {
        // Arrange
        _senderMock.Setup(x => x.Send(_command, CancellationToken.None)).ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(_command, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(x => x.Send(_command, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingUpdateCategory_ThenShouldCallSender()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateCategoryCommand>(), CancellationToken.None))
            .ReturnsAsync(Unit.Value);

        // Act
        await _endpoint.HandleAsync(_command, _senderMock.Object);

        // Assert
        _senderMock.Verify(x => x.Send(_command, CancellationToken.None), Times.Once);
    }
}
