using BookWorm.Catalog.Features.Categories.Create;
using BookWorm.SharedKernel.SeedWork;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Categories.Create;

public sealed class CreateCategoryEndpointTests
{
    private readonly CreateCategoryEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreateCategory_ThenShouldReturnCreatedResult()
    {
        // Arrange
        var command = new CreateCategoryCommand("Test Category");
        var expectedId = Guid.CreateVersion7();
        _senderMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _endpoint.HandleAsync(command, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Created<Guid>>();
        result.Value.ShouldBe(expectedId);

        var expectedUrl = new UrlBuilder()
            .WithVersion()
            .WithResource(nameof(Categories))
            .WithId(expectedId)
            .Build();
        result.Location.ShouldBe(expectedUrl);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreateCategory_ThenShouldCallSender()
    {
        // Arrange
        var command = new CreateCategoryCommand("Test Category");
        var expectedId = Guid.CreateVersion7();
        _senderMock
            .Setup(x => x.Send(It.IsAny<CreateCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        // Act
        await _endpoint.HandleAsync(command, _senderMock.Object);

        // Assert
        _senderMock.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}
