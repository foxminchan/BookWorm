using BookWorm.Catalog.Features.Categories.Delete;
using BookWorm.Chassis.Command;
using BookWorm.Chassis.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.UnitTests.Features.Categories.Delete;

public sealed class DeleteCategoryEndpointTests
{
    private readonly DeleteCategoryEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();
    private readonly Guid _validCategoryId = Guid.CreateVersion7();

    [Test]
    public async Task GivenValidCategoryId_WhenHandlingDeleteCategory_ThenShouldCallSenderAndReturnNoContent()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(_validCategoryId, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteCategoryCommand>(c => c.Id == _validCategoryId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidCategoryIdWithCancellationToken_WhenHandlingDeleteCategory_ThenShouldPassCancellationTokenToSender()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteCategoryCommand>(), cancellationToken))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(
            _validCategoryId,
            _senderMock.Object,
            cancellationToken
        );

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteCategoryCommand>(c => c.Id == _validCategoryId),
                    cancellationToken
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyGuidCategoryId_WhenHandlingDeleteCategory_ThenShouldSendCommandWithEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(emptyGuid, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteCategoryCommand>(c => c.Id == emptyGuid),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandlingDeleteCategory_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Category deletion failed");
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _endpoint.HandleAsync(_validCategoryId, _senderMock.Object)
        );

        exception.Message.ShouldBe("Category deletion failed");
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteCategoryCommand>(c => c.Id == _validCategoryId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenMultipleSequentialRequests_WhenHandlingDeleteCategory_ThenShouldHandleEachRequestIndependently()
    {
        // Arrange
        var categoryId1 = Guid.CreateVersion7();
        var categoryId2 = Guid.CreateVersion7();
        var categoryId3 = Guid.CreateVersion7();

        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result1 = await _endpoint.HandleAsync(categoryId1, _senderMock.Object);
        var result2 = await _endpoint.HandleAsync(categoryId2, _senderMock.Object);
        var result3 = await _endpoint.HandleAsync(categoryId3, _senderMock.Object);

        // Assert
        result1.ShouldBeOfType<NoContent>();
        result2.ShouldBeOfType<NoContent>();
        result3.ShouldBeOfType<NoContent>();

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteCategoryCommand>(c => c.Id == categoryId1),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteCategoryCommand>(c => c.Id == categoryId2),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteCategoryCommand>(c => c.Id == categoryId3),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenMultipleConcurrentRequests_WhenHandlingDeleteCategory_ThenShouldHandleEachRequestConcurrently()
    {
        // Arrange
        var categoryIds = new[]
        {
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
        };

        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var tasks = categoryIds.Select(id => _endpoint.HandleAsync(id, _senderMock.Object));
        var results = await Task.WhenAll(tasks);

        // Assert
        foreach (var result in results)
        {
            result.ShouldBeOfType<NoContent>();
        }

        foreach (var categoryId in categoryIds)
        {
            _senderMock.Verify(
                s =>
                    s.Send(
                        It.Is<DeleteCategoryCommand>(c => c.Id == categoryId),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        _senderMock.Verify(
            s => s.Send(It.IsAny<DeleteCategoryCommand>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3)
        );
    }

    [Test]
    public async Task GivenAlreadyCancelledToken_WhenHandlingDeleteCategory_ThenShouldThrowOperationCanceledException()
    {
        // Arrange
        var cancelledToken = new CancellationToken(true);
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _endpoint.HandleAsync(_validCategoryId, _senderMock.Object, cancelledToken)
        );

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteCategoryCommand>(c => c.Id == _validCategoryId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidRequest_WhenHandlingDeleteCategory_ThenShouldCreateCorrectCommandObject()
    {
        // Arrange
        DeleteCategoryCommand? capturedCommand = null;
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteCategoryCommand>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>(
                (command, _) => capturedCommand = command as DeleteCategoryCommand
            )
            .ReturnsAsync(Unit.Value);

        // Act
        await _endpoint.HandleAsync(_validCategoryId, _senderMock.Object);

        // Assert
        capturedCommand.ShouldNotBeNull();
        capturedCommand.Id.ShouldBe(_validCategoryId);
    }

    [Test]
    public void GivenDeleteCategoryEndpoint_WhenCheckingImplementedInterfaces_ThenShouldImplementCorrectInterface()
    {
        // Arrange & Act
        var endpoint = new DeleteCategoryEndpoint();

        // Assert
        endpoint.ShouldBeAssignableTo<IEndpoint<NoContent, Guid, ISender>>();
    }

    [Test]
    public void GivenCategoryId_WhenCreatingDeleteCategoryCommand_ThenPropertiesShouldBeCorrectlyInitialized()
    {
        // Arrange & Act
        var command = new DeleteCategoryCommand(_validCategoryId);

        // Assert
        command.Id.ShouldBe(_validCategoryId);
        command.ShouldBeAssignableTo<ICommand>();
    }

    [Test]
    public void GivenTwoDeleteCategoryCommandsWithSameId_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var command1 = new DeleteCategoryCommand(_validCategoryId);
        var command2 = new DeleteCategoryCommand(_validCategoryId);

        // Act & Assert
        command1.ShouldBe(command2);
        command1.GetHashCode().ShouldBe(command2.GetHashCode());
    }

    [Test]
    public void GivenTwoDeleteCategoryCommandsWithDifferentIds_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var categoryId1 = Guid.CreateVersion7();
        var categoryId2 = Guid.CreateVersion7();
        var command1 = new DeleteCategoryCommand(categoryId1);
        var command2 = new DeleteCategoryCommand(categoryId2);

        // Act & Assert
        command1.ShouldNotBe(command2);
        command1.GetHashCode().ShouldNotBe(command2.GetHashCode());
    }

    [Test]
    public void GivenTwoDeleteCategoryEndpointInstances_WhenComparing_ThenShouldBeDifferentInstances()
    {
        // Arrange
        var endpoint1 = new DeleteCategoryEndpoint();
        var endpoint2 = new DeleteCategoryEndpoint();

        // Act & Assert
        endpoint1.ShouldNotBeSameAs(endpoint2);
        endpoint1.ShouldNotBe(endpoint2);
    }

    [Test]
    public void GivenDeleteCategoryEndpoint_WhenCreatingInstance_ThenShouldNotThrowException()
    {
        // Arrange & Act & Assert
        Should.NotThrow(() => new DeleteCategoryEndpoint());
    }

    [Test]
    public async Task GivenSpecificGuidVersion_WhenHandlingDeleteCategory_ThenShouldHandleCorrectly()
    {
        // Arrange
        var version7Guid = Guid.CreateVersion7();
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _endpoint.HandleAsync(version7Guid, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteCategoryCommand>(c => c.Id == version7Guid),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
