using System.Reflection;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Domain.Settings;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.Infrastructure.Table;
using BookWorm.Notification.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using Quartz;

namespace BookWorm.Notification.UnitTests.Workers;

public class ResendErrorEmailWorkerTests : IDisposable
{
    private readonly Mock<ILogger<ResendErrorEmailWorker>> _loggerMock;
    private readonly string _partitionKey = nameof(Outbox).ToLowerInvariant();
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
        GC.SuppressFinalize(this);
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
            .Setup(x => x.ListAsync<Outbox>(_partitionKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        // Act
        await _worker.Execute(Mock.Of<IJobExecutionContext>());

        // Assert
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Exactly(failedEmails.Count)
        );

        _tableServiceMock.Verify(
            x => x.ListAsync<Outbox>(_partitionKey, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenFailedEmail_WhenResendingFails_ThenShouldLogError()
    {
        // Arrange
        var failedEmail = new Outbox("Test User", "test@example.com", "Subject", "Body");
        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>(_partitionKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync([failedEmail]);

        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Send failed"));

        // Act
        await _worker.Execute(Mock.Of<IJobExecutionContext>());

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
                        (o, t) => o.ToString()!.Contains("No failed emails found to resend")
                    ),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );

        _tableServiceMock.Verify(
            x => x.ListAsync<Outbox>(_partitionKey, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenException_WhenResending_ThenShouldLogError()
    {
        // Arrange
        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>(_partitionKey, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Table service error"));

        // Act
        await _worker.Execute(Mock.Of<IJobExecutionContext>());

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("Error occurred in job execution")
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
            .Setup(x => x.ListAsync<Outbox>(_partitionKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        _senderMock
            .SetupSequence(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("First email failed"))
            .Returns(Task.CompletedTask);

        // Act
        await _worker.Execute(Mock.Of<IJobExecutionContext>());

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
    public async Task GivenSemaphoreAcquired_WhenJobExecutes_ThenShouldSkipProcessing()
    {
        // Arrange
        var failedEmails = new List<Outbox>
        {
            new("Test User", "test@example.com", "Subject", "Body"),
        };

        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>(_partitionKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        // Get the private semaphore field using reflection
        var semaphoreField = typeof(ResendErrorEmailWorker).GetField(
            "_semaphore",
            BindingFlags.NonPublic | BindingFlags.Instance
        );
        var semaphore = (SemaphoreSlim)semaphoreField!.GetValue(_worker)!;

        // Acquire the semaphore
        await semaphore.WaitAsync(0);

        try
        {
            // Act
            await _worker.Execute(Mock.Of<IJobExecutionContext>());

            // Assert
            _tableServiceMock.Verify(
                x => x.ListAsync<Outbox>(_partitionKey, It.IsAny<CancellationToken>()),
                Times.Never
            );

            _senderMock.Verify(
                x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
        finally
        {
            // Release the semaphore
            semaphore.Release();
        }
    }
}
