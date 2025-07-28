using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure;
using BookWorm.Notification.Infrastructure.Senders;
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
    private readonly Mock<INotificationDbContext> _dbContextMock;
    private readonly Mock<ILogger<ResendErrorEmailWorker>> _loggerMock;
    private readonly Mock<ISender> _senderMock;
    private readonly ResendErrorEmailWorker _worker;

    public ResendErrorEmailWorkerTests()
    {
        _loggerMock = new();
        Mock<GlobalLogBuffer> logBufferMock = new();
        Mock<IServiceScopeFactory> scopeFactoryMock = new();
        Mock<IServiceScope> scopeMock = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        _dbContextMock = new();
        _senderMock = new();

        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
        serviceProviderMock
            .Setup(x => x.GetService(typeof(INotificationDbContext)))
            .Returns(_dbContextMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ISender))).Returns(_senderMock.Object);

        _worker = new(_loggerMock.Object, logBufferMock.Object, scopeFactoryMock.Object);
    }

    [Test]
    public async Task GivenFailedEmails_WhenResending_ThenShouldProcessAllEmails()
    {
        // Arrange
        var failedEmails = TestDataFakers.Outbox.Generate(2);

        var mockDbSet = failedEmails.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Outboxes).Returns(mockDbSet.Object);

        // Act
        await _worker.Execute(Mock.Of<IJobExecutionContext>());

        // Assert
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Exactly(failedEmails.Count)
        );
    }

    [Test]
    public async Task GivenNoFailedEmails_WhenResending_ThenShouldNotSendAnyEmails()
    {
        // Arrange
        var emptyList = new List<Outbox>();
        var mockDbSet = emptyList.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Outboxes).Returns(mockDbSet.Object);

        // Act
        await _worker.Execute(Mock.Of<IJobExecutionContext>());

        // Assert
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenFailedEmail_WhenResendingFails_ThenShouldLogError()
    {
        // Arrange
        var failedEmail = TestDataFakers.Outbox.Generate();

        var mockDbSet = new[] { failedEmail }.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Outboxes).Returns(mockDbSet.Object);

        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Send failed"));

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
}
