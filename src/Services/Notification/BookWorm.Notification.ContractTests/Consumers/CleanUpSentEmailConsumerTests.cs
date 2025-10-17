using BookWorm.Chassis.Repository;
using BookWorm.Chassis.Specification;
using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Logging;

namespace BookWorm.Notification.ContractTests.Consumers;

public sealed class CleanUpSentEmailConsumerTests : SnapshotTestBase
{
    private readonly Mock<GlobalLogBuffer> _logBufferMock;
    private readonly Mock<ILogger<CleanUpSentEmailIntegrationEventHandler>> _loggerMock;
    private readonly Mock<IOutboxRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public CleanUpSentEmailConsumerTests()
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

        var command = new CleanUpSentEmailIntegrationEvent();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<CleanUpSentEmailIntegrationEventHandler>()
            )
            .AddScoped(_ => _loggerMock.Object)
            .AddScoped(_ => _logBufferMock.Object)
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CleanUpSentEmailIntegrationEventHandler>();
        (await consumerHarness.Consumed.Any<CleanUpSentEmailIntegrationEvent>()).ShouldBeTrue();

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

        // Contract verification - CleanUpSentEmailIntegrationEvent has no parameters so it's deterministic
        await VerifySnapshot(command);

        await harness.Stop();
    }

    [Test]
    public async Task GivenNoSentEmailsExist_WhenHandlingCleanUpEvent_ThenShouldLogNoEmailsFound()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var command = new CleanUpSentEmailIntegrationEvent();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<CleanUpSentEmailIntegrationEventHandler>()
            )
            .AddScoped(_ => _loggerMock.Object)
            .AddScoped(_ => _logBufferMock.Object)
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CleanUpSentEmailIntegrationEventHandler>();
        (await consumerHarness.Consumed.Any<CleanUpSentEmailIntegrationEvent>()).ShouldBeTrue();

        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(x => x.BulkDelete(It.IsAny<IEnumerable<Outbox>>()), Times.Never);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        VerifyLogMessage(LogLevel.Debug, "Starting cleanup of sent emails", Times.Once());
        VerifyLogMessage(LogLevel.Debug, "No sent emails found for cleanup", Times.Once());

        // Contract verification - CleanUpSentEmailIntegrationEvent has no parameters so it's deterministic
        await VerifySnapshot(command);

        await harness.Stop();
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

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<CleanUpSentEmailIntegrationEventHandler>()
            )
            .AddScoped(_ => _loggerMock.Object)
            .AddScoped(_ => _logBufferMock.Object)
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CleanUpSentEmailIntegrationEventHandler>();
        (await consumerHarness.Consumed.Any<CleanUpSentEmailIntegrationEvent>()).ShouldBeTrue();

        // Verify that the consumer faulted
        var consumedMessage = consumerHarness
            .Consumed.Select<CleanUpSentEmailIntegrationEvent>()
            .FirstOrDefault();
        consumedMessage.ShouldNotBeNull();
        consumedMessage.Exception.ShouldNotBeNull();
        consumedMessage.Exception.ShouldBeOfType<InvalidOperationException>();
        consumedMessage.Exception.Message.ShouldBe("Failed to clean up sent emails");
        consumedMessage.Exception.InnerException.ShouldNotBeNull();
        consumedMessage.Exception.InnerException.ShouldBeOfType<InvalidOperationException>();
        consumedMessage.Exception.InnerException.Message.ShouldBe("Database connection failed");

        _logBufferMock.Verify(x => x.Flush(), Times.Once);

        VerifyLogMessageWithException(
            LogLevel.Error,
            "Error occurred while cleaning up sent emails",
            exception,
            Times.Once()
        );

        await harness.Stop();
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

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<CleanUpSentEmailIntegrationEventHandler>()
            )
            .AddScoped(_ => _loggerMock.Object)
            .AddScoped(_ => _logBufferMock.Object)
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CleanUpSentEmailIntegrationEventHandler>();
        (await consumerHarness.Consumed.Any<CleanUpSentEmailIntegrationEvent>()).ShouldBeTrue();

        // Verify that the consumer faulted
        var consumedMessage = consumerHarness
            .Consumed.Select<CleanUpSentEmailIntegrationEvent>()
            .FirstOrDefault();
        consumedMessage.ShouldNotBeNull();
        consumedMessage.Exception.ShouldNotBeNull();
        consumedMessage.Exception.ShouldBeOfType<InvalidOperationException>();
        consumedMessage.Exception.Message.ShouldBe("Failed to clean up sent emails");
        consumedMessage.Exception.InnerException.ShouldNotBeNull();
        consumedMessage.Exception.InnerException.ShouldBeOfType<InvalidOperationException>();
        consumedMessage.Exception.InnerException.Message.ShouldBe("Failed to save changes");

        _repositoryMock.Verify(x => x.BulkDelete(It.IsAny<IEnumerable<Outbox>>()), Times.Once);

        _logBufferMock.Verify(x => x.Flush(), Times.Once);

        VerifyLogMessageWithException(
            LogLevel.Error,
            "Error occurred while cleaning up sent emails",
            exception,
            Times.Once()
        );

        await harness.Stop();
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

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<CleanUpSentEmailIntegrationEventHandler>()
            )
            .AddScoped(_ => _loggerMock.Object)
            .AddScoped(_ => _logBufferMock.Object)
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CleanUpSentEmailIntegrationEventHandler>();
        (await consumerHarness.Consumed.Any<CleanUpSentEmailIntegrationEvent>()).ShouldBeTrue();

        capturedSpec.ShouldNotBeNull();

        await harness.Stop();
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

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<CleanUpSentEmailIntegrationEventHandler>()
            )
            .AddScoped(_ => _loggerMock.Object)
            .AddScoped(_ => _logBufferMock.Object)
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CleanUpSentEmailIntegrationEventHandler>();
        (await consumerHarness.Consumed.Any<CleanUpSentEmailIntegrationEvent>()).ShouldBeTrue();

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

        await harness.Stop();
    }

    [Test]
    public async Task GivenValidEvent_WhenHandlingCleanUpEvent_ThenShouldCompleteSuccessfully()
    {
        // Arrange
        List<Outbox> sentEmails = [new("User One", "user1@example.com", "Subject 1", "Body 1")];

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Set up the repository to work with any cancellation token, not just the specific one
        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sentEmails);

        var command = new CleanUpSentEmailIntegrationEvent();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<CleanUpSentEmailIntegrationEventHandler>()
            )
            .AddScoped(_ => _loggerMock.Object)
            .AddScoped(_ => _logBufferMock.Object)
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command, cancellationToken);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CleanUpSentEmailIntegrationEventHandler>();
        (
            await consumerHarness.Consumed.Any<CleanUpSentEmailIntegrationEvent>(cancellationToken)
        ).ShouldBeTrue();

        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(x => x.BulkDelete(It.IsAny<IEnumerable<Outbox>>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        await harness.Stop(cancellationToken);
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

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<CleanUpSentEmailIntegrationEventHandler>()
            )
            .AddScoped(_ => _loggerMock.Object)
            .AddScoped(_ => _logBufferMock.Object)
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<CleanUpSentEmailIntegrationEventHandler>();
        (await consumerHarness.Consumed.Any<CleanUpSentEmailIntegrationEvent>()).ShouldBeTrue();

        // Verify that the consumer faulted
        var consumedMessage = consumerHarness
            .Consumed.Select<CleanUpSentEmailIntegrationEvent>()
            .FirstOrDefault();
        consumedMessage.ShouldNotBeNull();
        consumedMessage.Exception.ShouldNotBeNull();
        consumedMessage.Exception.ShouldBeOfType<InvalidOperationException>();
        consumedMessage.Exception.Message.ShouldBe("Failed to clean up sent emails");
        consumedMessage.Exception.InnerException.ShouldNotBeNull();
        consumedMessage.Exception.InnerException.ShouldBeOfType<InvalidOperationException>();
        consumedMessage.Exception.InnerException.Message.ShouldBe("SQL Server connection timeout");

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

        await harness.Stop();
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
