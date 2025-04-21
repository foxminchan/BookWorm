using System.Reflection;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Table;
using BookWorm.Notification.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookWorm.Notification.UnitTests.Workers;

public sealed class CleanUpSentEmailWorkerTests : IDisposable
{
    private readonly Mock<ILogger<CleanUpSentEmailWorker>> _loggerMock;
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<ITableService> _tableServiceMock;
    private readonly CleanUpSentEmailWorker _worker;

    public CleanUpSentEmailWorkerTests()
    {
        _loggerMock = new();
        _tableServiceMock = new();

        // Create a service collection and add our mock service
        var services = new ServiceCollection();
        services.AddSingleton(_tableServiceMock.Object);

        // Create a service provider
        _serviceProvider = services.BuildServiceProvider();

        // Create a scope factory that returns a scope with our service provider
        var scopeFactory = new Mock<IServiceScopeFactory>();
        var scope = new Mock<IServiceScope>();
        scope.Setup(x => x.ServiceProvider).Returns(_serviceProvider);
        scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);

        _worker = new(_loggerMock.Object, scopeFactory.Object);
    }

    public void Dispose()
    {
        _worker.Dispose();
        _serviceProvider.Dispose();
    }

    [Test]
    public async Task GivenWorker_WhenStarting_ThenShouldSetTimerCorrectly()
    {
        // Act
        await _worker.StartAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("Clean Up Sent Email Worker is starting")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenWorker_WhenStopping_ThenShouldStopTimer()
    {
        // Act
        await _worker.StopAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("Clean Up Sent Email Worker is stopping")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNoSentEmails_WhenCleaningUp_ThenShouldLogNoEmailsFound()
    {
        // Arrange
        var emptyEmails = new List<Outbox>();
        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>("outbox", It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyEmails);

        // Act
        var method = _worker
            .GetType()
            .GetMethod("CleanUpSentEmails", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)method!.Invoke(_worker, null)!;

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("No sent emails found to delete")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSentEmails_WhenCleaningUp_ThenShouldDeleteAllSentEmails()
    {
        // Arrange
        var sentEmails = new List<Outbox>
        {
            new("Test1", "test1@example.com", "Subject1", "Body1"),
            new("Test2", "test2@example.com", "Subject2", "Body2"),
        };

        // Mark IsSent as true for all emails
        foreach (var email in sentEmails)
        {
            email.MarkAsSent();
        }

        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>("outbox", It.IsAny<CancellationToken>()))
            .ReturnsAsync(sentEmails);

        // Act
        var method = _worker
            .GetType()
            .GetMethod("CleanUpSentEmails", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)method!.Invoke(_worker, null)!;

        // Assert
        _tableServiceMock.Verify(
            x => x.DeleteAsync("outbox", It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("Found 2 sent emails to delete")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenMixedEmails_WhenCleaningUp_ThenShouldOnlyDeleteSentEmails()
    {
        // Arrange
        var emails = new List<Outbox>
        {
            new("Test1", "test1@example.com", "Subject1", "Body1"),
            new("Test2", "test2@example.com", "Subject2", "Body2"),
        };

        // Mark IsSent as true for the first email
        emails[0].MarkAsSent();

        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>("outbox", It.IsAny<CancellationToken>()))
            .ReturnsAsync(emails);

        // Act
        var method = _worker
            .GetType()
            .GetMethod("CleanUpSentEmails", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)method!.Invoke(_worker, null)!;

        // Assert
        _tableServiceMock.Verify(
            x => x.DeleteAsync("outbox", It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenLargeNumberOfEmails_WhenCleaningUp_ThenShouldProcessInBatches()
    {
        // Arrange
        var sentEmails = new List<Outbox>();
        for (var i = 0; i < 150; i++)
        {
            var email = new Outbox($"Test{i}", $"test{i}@example.com", $"Subject{i}", $"Body{i}");
            email.MarkAsSent();
            sentEmails.Add(email);
        }

        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>("outbox", It.IsAny<CancellationToken>()))
            .ReturnsAsync(sentEmails);

        // Act
        var method = _worker
            .GetType()
            .GetMethod("CleanUpSentEmails", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)method!.Invoke(_worker, null)!;

        // Assert
        _tableServiceMock.Verify(
            x => x.DeleteAsync("outbox", It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(150)
        );
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("Successfully deleted batch of")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );
    }

    [Test]
    public async Task GivenWorker_WhenDisposed_ThenShouldCleanupResources()
    {
        // Arrange
        await _worker.StartAsync(CancellationToken.None);

        // Act
        await _worker.StopAsync(CancellationToken.None);
        _worker.Dispose();

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("Clean Up Sent Email Worker is stopping")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenWorker_WhenStarting_ThenShouldSetTimerToRunAtMidnight()
    {
        // Act
        await _worker.StartAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("Clean Up Sent Email Worker is starting")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );
    }
}
