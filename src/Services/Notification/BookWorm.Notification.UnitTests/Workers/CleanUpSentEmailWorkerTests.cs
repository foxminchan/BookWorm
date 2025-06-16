using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Table;
using BookWorm.Notification.UnitTests.Fakers;
using BookWorm.Notification.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BookWorm.Notification.UnitTests.Workers;

public sealed class CleanUpSentEmailWorkerTests : IDisposable
{
    private readonly Mock<ILogger<CleanUpSentEmailWorker>> _loggerMock;
    private readonly string _partitionKey = nameof(Outbox).ToLowerInvariant();
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<ITableService> _tableServiceMock;
    private readonly CleanUpSentEmailWorker _worker;

    public CleanUpSentEmailWorkerTests()
    {
        _loggerMock = new();
        Mock<GlobalLogBuffer> logBufferMock = new();
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

        _worker = new(_loggerMock.Object, logBufferMock.Object, scopeFactory.Object);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    [Test]
    public async Task GivenNoSentEmails_WhenCleaningUp_ThenShouldLogNoEmailsFound()
    {
        // Arrange
        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>(_partitionKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _worker.Execute(Mock.Of<IJobExecutionContext>());

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
        var sentEmails = new List<Outbox>();
        for (var i = 0; i < 2; i++)
        {
            sentEmails.Add(TestDataFakers.Outbox.AsSent());
        }

        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>(_partitionKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sentEmails);

        // Act
        await _worker.Execute(Mock.Of<IJobExecutionContext>());

        // Assert
        _tableServiceMock.Verify(
            x => x.DeleteAsync(_partitionKey, It.IsAny<string>(), It.IsAny<CancellationToken>()),
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
        var sentOutbox = TestDataFakers.Outbox.AsSent();
        var unsentOutbox = TestDataFakers.Outbox.Generate();

        var emails = new List<Outbox> { sentOutbox, unsentOutbox };

        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>(_partitionKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emails);

        // Act
        await _worker.Execute(Mock.Of<IJobExecutionContext>());

        // Assert
        _tableServiceMock.Verify(
            x => x.DeleteAsync(_partitionKey, It.IsAny<string>(), It.IsAny<CancellationToken>()),
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
            sentEmails.Add(TestDataFakers.Outbox.AsSent());
        }

        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>(_partitionKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sentEmails);

        // Act
        await _worker.Execute(Mock.Of<IJobExecutionContext>());

        // Assert
        _tableServiceMock.Verify(
            x => x.DeleteAsync(_partitionKey, It.IsAny<string>(), It.IsAny<CancellationToken>()),
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
    public async Task GivenException_WhenCleaningUp_ThenShouldLogError()
    {
        // Arrange
        var exception = new Exception("Table service error");
        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>(_partitionKey, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Should.ThrowAsync<Exception>(async () =>
            await _worker.Execute(Mock.Of<IJobExecutionContext>())
        );

        thrownException.Message.ShouldBe("Table service error");

        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("Error occurred in job execution")
                    ),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}
