using BookWorm.Chassis.Repository;
using BookWorm.Chassis.Specification;
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
    private CleanUpSentEmailIntegrationEventHandler _handler = null!;

    [Before(Test)]
    public void SetUp()
    {
        _loggerMock = new();
        _logBufferMock = new();
        _repositoryMock = new();
        _unitOfWorkMock = new();

        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _handler = new(_loggerMock.Object, _logBufferMock.Object, _repositoryMock.Object);
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

        var command = new CleanUpSentEmailIntegrationEvent();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(command);

        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(
            x => x.BulkDelete(It.Is<IEnumerable<Outbox>>(emails => emails.Count() == 3)),
            Times.Once
        );

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        VerifyLogMessage(LogLevel.Debug, "Starting cleanup of sent emails", Times.Once());
        VerifyLogMessage(LogLevel.Debug, "Found 3 sent emails to delete", Times.Once());
        VerifyLogMessage(LogLevel.Information, "Successfully cleaned up sent emails", Times.Once());
    }

    [Test]
    public async Task GivenNoSentEmailsExist_WhenHandlingCleanUpEvent_ThenShouldLogNoEmailsFound()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var command = new CleanUpSentEmailIntegrationEvent();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(command);

        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(x => x.BulkDelete(It.IsAny<IEnumerable<Outbox>>()), Times.Never);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        VerifyLogMessage(LogLevel.Debug, "Starting cleanup of sent emails", Times.Once());
        VerifyLogMessage(LogLevel.Debug, "No sent emails found for cleanup", Times.Once());
    }

    [Test]
    public async Task GivenRepositoryThrowsExceptionOnList_WhenHandlingCleanUpEvent_ThenShouldLogErrorAndRethrow()
    {
        // Arrange
        var exception = new InvalidOperationException("Database connection failed");

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var command = new CleanUpSentEmailIntegrationEvent();

        // Act & Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );

        ex.Message.ShouldBe("Failed to clean up sent emails");
        ex.InnerException.ShouldNotBeNull();
        ex.InnerException.ShouldBeOfType<InvalidOperationException>();
        ex.InnerException!.Message.ShouldBe("Database connection failed");

        _logBufferMock.Verify(x => x.Flush(), Times.Once);

        VerifyLogMessageWithException(
            LogLevel.Error,
            "Error occurred while cleaning up sent emails",
            exception,
            Times.Once()
        );
    }

    [Test]
    public async Task GivenSaveChangesThrowsException_WhenHandlingCleanUpEvent_ThenShouldLogErrorAndRethrow()
    {
        // Arrange
        List<Outbox> sentEmails = [new("User One", "user1@example.com", "Subject 1", "Body 1")];

        var exception = new InvalidOperationException("Failed to save changes");

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sentEmails);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var command = new CleanUpSentEmailIntegrationEvent();

        // Act & Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );

        ex.Message.ShouldBe("Failed to clean up sent emails");
        ex.InnerException.ShouldNotBeNull();
        ex.InnerException.ShouldBeOfType<InvalidOperationException>();
        ex.InnerException!.Message.ShouldBe("Failed to save changes");

        _repositoryMock.Verify(x => x.BulkDelete(It.IsAny<IEnumerable<Outbox>>()), Times.Once);

        _logBufferMock.Verify(x => x.Flush(), Times.Once);

        VerifyLogMessageWithException(
            LogLevel.Error,
            "Error occurred while cleaning up sent emails",
            exception,
            Times.Once()
        );
    }

    [Test]
    public async Task GivenValidEvent_WhenHandling_ThenShouldUseCorrectFilterSpec()
    {
        // Arrange
        OutboxFilterSpec? capturedSpec = null;

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .Callback<ISpecification<Outbox>, CancellationToken>(
                (spec, _) =>
                {
                    if (spec is OutboxFilterSpec filterSpec)
                    {
                        capturedSpec = filterSpec;
                    }
                }
            )
            .ReturnsAsync([]);

        var command = new CleanUpSentEmailIntegrationEvent();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedSpec.ShouldNotBeNull();
    }

    [Test]
    public async Task GivenMultipleBatches_WhenHandlingCleanUpEvent_ThenShouldDeleteAllEmailsCorrectly()
    {
        // Arrange
        List<Outbox> sentEmails = [];
        for (var i = 1; i <= 10; i++)
        {
            sentEmails.Add(new($"User {i}", $"user{i}@example.com", $"Subject {i}", $"Body {i}"));
        }

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sentEmails);

        var command = new CleanUpSentEmailIntegrationEvent();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(
            x => x.BulkDelete(It.Is<IEnumerable<Outbox>>(emails => emails.Count() == 10)),
            Times.Once
        );

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        VerifyLogMessage(LogLevel.Debug, "Starting cleanup of sent emails", Times.Once());
        VerifyLogMessage(LogLevel.Debug, "Found 10 sent emails to delete", Times.Once());
        VerifyLogMessage(LogLevel.Information, "Successfully cleaned up sent emails", Times.Once());
    }

    [Test]
    public async Task GivenValidEvent_WhenHandlingCleanUpEvent_ThenShouldCompleteSuccessfully()
    {
        // Arrange
        List<Outbox> sentEmails = [new("User One", "user1@example.com", "Subject 1", "Body 1")];

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sentEmails);

        var command = new CleanUpSentEmailIntegrationEvent();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(x => x.BulkDelete(It.IsAny<IEnumerable<Outbox>>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenSqlExceptionDuringBulkDelete_WhenHandlingCleanUpEvent_ThenShouldLogErrorAndRethrow()
    {
        // Arrange
        List<Outbox> sentEmails = [new("User One", "user1@example.com", "Subject 1", "Body 1")];

        var exception = new InvalidOperationException("SQL Server connection timeout");

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sentEmails);

        _repositoryMock.Setup(x => x.BulkDelete(It.IsAny<IEnumerable<Outbox>>())).Throws(exception);

        var command = new CleanUpSentEmailIntegrationEvent();

        // Act & Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );

        ex.Message.ShouldBe("Failed to clean up sent emails");
        ex.InnerException.ShouldNotBeNull();
        ex.InnerException.ShouldBeOfType<InvalidOperationException>();
        ex.InnerException!.Message.ShouldBe("SQL Server connection timeout");

        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(x => x.BulkDelete(It.IsAny<IEnumerable<Outbox>>()), Times.Once);

        // SaveChanges should never be called when BulkDelete fails
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _logBufferMock.Verify(x => x.Flush(), Times.Once);

        VerifyLogMessageWithException(
            LogLevel.Error,
            "Error occurred while cleaning up sent emails",
            exception,
            Times.Once()
        );
    }

    private void VerifyLogMessage(LogLevel level, string expectedMessage, Times times)
    {
        _loggerMock.Verify(
            x =>
                x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            times
        );
    }

    private void VerifyLogMessageWithException<TException>(
        LogLevel level,
        string expectedMessage,
        TException expectedException,
        Times times
    )
        where TException : Exception
    {
        _loggerMock.Verify(
            x =>
                x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedMessage)),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            times
        );
    }
}
