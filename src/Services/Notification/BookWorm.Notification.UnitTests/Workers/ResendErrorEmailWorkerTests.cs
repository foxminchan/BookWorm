using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Domain.Settings;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.Infrastructure.Table;
using BookWorm.Notification.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace BookWorm.Notification.UnitTests.Workers;

public class ResendErrorEmailWorkerTests : IDisposable
{
    private readonly Mock<ILogger<ResendErrorEmailWorker>> _loggerMock;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<ITableService> _tableServiceMock;
    private readonly ResendErrorEmailWorker _worker;

    public ResendErrorEmailWorkerTests()
    {
        _loggerMock = new();
        Mock<IServiceScopeFactory> scopeFactoryMock = new();
        Mock<IServiceScope> scopeMock = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        _tableServiceMock = new();
        _senderMock = new();

        EmailOptions emailOptions = new() { From = "test@example.com", Name = "Test Sender" };

        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
        serviceProviderMock
            .Setup(x => x.GetService(typeof(ITableService)))
            .Returns(_tableServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ISender))).Returns(_senderMock.Object);

        _worker = new(_loggerMock.Object, emailOptions, scopeFactoryMock.Object);
    }

    public void Dispose()
    {
        _worker.Dispose();
    }

    [Test]
    public async Task GivenWorker_WhenStarting_ThenShouldInitializeTimer()
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
                        (o, t) => o.ToString()!.Contains("Resend Error Email Service is starting")
                    ),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenFailedEmails_WhenResending_ThenShouldResendAllEmails()
    {
        // Arrange
        var failedEmails = new List<Outbox>
        {
            new("Test User", "test@example.com", "Subject", "Body"),
            new("Test User 2", "test2@example.com", "Subject 2", "Body 2"),
        };

        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>("outbox", It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        // Act
        await _worker.StartAsync(CancellationToken.None);
        await Task.Delay(100);

        // Assert
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
    }

    [Test]
    public async Task GivenFailedEmail_WhenResendingFails_ThenShouldLogError()
    {
        // Arrange
        var failedEmail = new Outbox("Test User", "test@example.com", "Subject", "Body");
        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>("outbox", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Outbox> { failedEmail });

        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Send failed"));

        // Act
        await _worker.StartAsync(CancellationToken.None);
        await Task.Delay(100);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Failed to resend email")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNoFailedEmails_WhenResending_ThenShouldLogNoEmailsFound()
    {
        // Arrange
        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>("outbox", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Outbox>());

        // Act
        await _worker.StartAsync(CancellationToken.None);
        await Task.Delay(100);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("No failed emails found to resend")
                    ),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenTimerCallback_WhenExceptionOccurs_ThenShouldLogError()
    {
        // Arrange
        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>("outbox", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Table service error"));

        // Act
        await _worker.StartAsync(CancellationToken.None);
        await Task.Delay(100);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("Error occurred in timer callback")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenFailedEmail_WhenIndividualEmailResendFails_ThenShouldLogErrorAndContinue()
    {
        // Arrange
        var failedEmails = new List<Outbox>
        {
            new("Test User", "test@example.com", "Subject", "Body"),
            new("Test User 2", "test2@example.com", "Subject 2", "Body 2"),
        };

        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>("outbox", It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        _senderMock
            .SetupSequence(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("First email failed"))
            .Returns(Task.CompletedTask);

        // Act
        await _worker.StartAsync(CancellationToken.None);
        await Task.Delay(100);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Failed to resend email")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );

        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
    }

    [Test]
    public async Task GivenWorker_WhenStopping_ThenShouldStopTimer()
    {
        // Arrange
        await _worker.StartAsync(CancellationToken.None);

        // Act
        await _worker.StopAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("Resend Error Email Service is stopping")
                    ),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCancellationToken_WhenWaitingForSemaphore_ThenShouldRespectCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        await _worker.StartAsync(cts.Token);
        await Task.Delay(100, cts.Token);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) =>
                            o.ToString()!.Contains("Previous resend operation is still running")
                    ),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}
