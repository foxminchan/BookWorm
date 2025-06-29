using System.Reflection;
using BookWorm.Notification.Domain.Exceptions;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.Infrastructure.Table;
using BookWorm.Notification.UnitTests.Fakers;
using BookWorm.Notification.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Logging;
using MimeKit;
using Quartz;

namespace BookWorm.Notification.UnitTests.Workers;

public class ResendErrorEmailWorkerTests
{
    private readonly Mock<ILogger<ResendErrorEmailWorker>> _loggerMock;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<ITableService> _tableServiceMock;
    private readonly ResendErrorEmailWorker _worker;

    public ResendErrorEmailWorkerTests()
    {
        _loggerMock = new();
        Mock<GlobalLogBuffer> logBufferMock = new();
        Mock<IServiceScopeFactory> scopeFactoryMock = new();
        Mock<IServiceScope> scopeMock = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        _tableServiceMock = new();
        _senderMock = new();

        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
        serviceProviderMock
            .Setup(x => x.GetService(typeof(ITableService)))
            .Returns(_tableServiceMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ISender))).Returns(_senderMock.Object);

        _worker = new(_loggerMock.Object, logBufferMock.Object, scopeFactoryMock.Object);
    }

    [Test]
    public async Task GivenFailedEmails_WhenResending_ThenShouldResendAllEmails()
    {
        // Arrange
        var failedEmails = TestDataFakers.Outbox.Generate(2);

        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>(TablePartition.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        // Act
        await _worker.Execute(Mock.Of<IJobExecutionContext>());

        // Assert
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Exactly(failedEmails.Count)
        );

        _tableServiceMock.Verify(
            x => x.ListAsync<Outbox>(TablePartition.Pending, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenFailedEmail_WhenResendingFails_ThenShouldLogError()
    {
        // Arrange
        var failedEmail = TestDataFakers.Outbox.Generate();

        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>(TablePartition.Pending, It.IsAny<CancellationToken>()))
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
            .Setup(x => x.ListAsync<Outbox>(TablePartition.Pending, It.IsAny<CancellationToken>()))
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
            x => x.ListAsync<Outbox>(TablePartition.Pending, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenException_WhenResending_ThenShouldLogError()
    {
        // Arrange
        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>(TablePartition.Pending, It.IsAny<CancellationToken>()))
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

        // Verify that the debug message was logged before the exception
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("Checking for failed emails to resend")
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
        var failedEmails = TestDataFakers.Outbox.Generate(2);

        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>(TablePartition.Pending, It.IsAny<CancellationToken>()))
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
        var failedEmails = TestDataFakers.Outbox.Generate(1);

        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>(TablePartition.Pending, It.IsAny<CancellationToken>()))
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
                x => x.ListAsync<Outbox>(TablePartition.Pending, It.IsAny<CancellationToken>()),
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

    [Test]
    public async Task GivenFailedEmail_WhenSenderThrowsNotificationExceptionWithInnerException_ThenShouldLogError()
    {
        // Arrange
        var failedEmail = TestDataFakers.Outbox.Generate();

        _tableServiceMock
            .Setup(x => x.ListAsync<Outbox>(TablePartition.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync([failedEmail]);

        var innerException = new InvalidOperationException("SMTP connection failed");
        var notificationException = new NotificationException(
            "Failed to send email. Exception: SMTP connection failed",
            innerException
        );

        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(notificationException);

        // Act
        await _worker.Execute(Mock.Of<IJobExecutionContext>());

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Failed to resend email")),
                    It.Is<Exception>(ex =>
                        ex is NotificationException && ex.InnerException != null
                    ),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}
