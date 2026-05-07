using BookWorm.Chassis.Repository;
using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Logging;

namespace BookWorm.Notification.ContractTests.Consumers;

public sealed class CleanUpSentEmailConsumerTests
{
    private Mock<GlobalLogBuffer> _logBufferMock = null!;
    private Mock<ILogger<CleanUpSentEmailIntegrationEventHandler>> _loggerMock = null!;
    private Mock<IOutboxRepository> _repositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;

    [Before(Test)]
    public void SetUp()
    {
        _loggerMock = new();
        _logBufferMock = new();
        _repositoryMock = new();
        _unitOfWorkMock = new();
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task GivenSentEmailsExist_WhenHandlingCleanUpEvent_ThenShouldDeleteEmailsAndSaveChanges()
    {
        // Arrange
        List<Outbox> sentEmails =
        [
            new("User One", "user1@example.com", "Subject 1", "Body 1"),
            new("User Two", "user2@example.com", "Subject 2", "Body 2"),
            new("User Three", "user3@example.com", "Subject 3", "Body 3"),
        ];

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sentEmails);

        var @event = new CleanUpSentEmailIntegrationEvent();
        var handler = new CleanUpSentEmailIntegrationEventHandler(
            _loggerMock.Object,
            _logBufferMock.Object,
            _repositoryMock.Object
        );

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.VerifyCloudEvent(@event);
        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            x => x.BulkDelete(It.Is<IEnumerable<Outbox>>(emails => emails.Count() == 3)),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenNoSentEmailsExist_WhenHandlingCleanUpEvent_ThenShouldLogNoEmailsFound()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var @event = new CleanUpSentEmailIntegrationEvent();
        var handler = new CleanUpSentEmailIntegrationEventHandler(
            _loggerMock.Object,
            _logBufferMock.Object,
            _repositoryMock.Object
        );

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.VerifyCloudEvent(@event);
        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(x => x.BulkDelete(It.IsAny<IEnumerable<Outbox>>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GivenRepositoryThrowsExceptionOnList_WhenHandlingCleanUpEvent_ThenShouldLogErrorAndRethrow()
    {
        // Arrange
        var innerException = new InvalidOperationException("Database connection failed");
        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(innerException);

        var @event = new CleanUpSentEmailIntegrationEvent();
        var handler = new CleanUpSentEmailIntegrationEventHandler(
            _loggerMock.Object,
            _logBufferMock.Object,
            _repositoryMock.Object
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.Handle(@event, CancellationToken.None)
        );

        ex.ShouldNotBeNull();
        ex.Message.ShouldBe("Failed to clean up sent emails");
        ex.InnerException.ShouldNotBeNull();
        ex.InnerException.ShouldBeOfType<InvalidOperationException>();
        ex.InnerException.Message.ShouldBe("Database connection failed");
        _logBufferMock.Verify(x => x.Flush(), Times.Once);
    }
}
