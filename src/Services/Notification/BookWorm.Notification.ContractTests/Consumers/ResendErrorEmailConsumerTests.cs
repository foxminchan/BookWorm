using BookWorm.Chassis.Repository;
using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace BookWorm.Notification.ContractTests.Consumers;

public sealed class ResendErrorEmailConsumerTests
{
    private Mock<GlobalLogBuffer> _logBufferMock = null!;
    private Mock<ILogger<ResendErrorEmailIntegrationEventHandler>> _loggerMock = null!;
    private Mock<IOutboxRepository> _repositoryMock = null!;
    private Mock<ISender> _senderMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;

    [Before(Test)]
    public void SetUp()
    {
        _logBufferMock = new();
        _loggerMock = new();
        _repositoryMock = new();
        _senderMock = new();
        _unitOfWorkMock = new();
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task GivenEmailsExist_WhenHandlingResendEvent_ThenShouldResendAllEmails()
    {
        // Arrange
        List<Outbox> failedEmails =
        [
            new("User One", "user1@example.com", "Subject 1", "Body 1"),
            new("User Two", "user2@example.com", "Subject 2", "Body 2"),
            new("User Three", "user3@example.com", "Subject 3", "Body 3"),
        ];

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var @event = new ResendErrorEmailIntegrationEvent();
        var handler = new ResendErrorEmailIntegrationEventHandler(
            _loggerMock.Object,
            _logBufferMock.Object,
            _repositoryMock.Object,
            _senderMock.Object
        );

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(@event);
        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3)
        );
        _logBufferMock.Verify(x => x.Flush(), Times.Never);
    }

    [Test]
    public async Task GivenNoEmailsExist_WhenHandlingResendEvent_ThenShouldCompleteWithoutSending()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var @event = new ResendErrorEmailIntegrationEvent();
        var handler = new ResendErrorEmailIntegrationEventHandler(
            _loggerMock.Object,
            _logBufferMock.Object,
            _repositoryMock.Object,
            _senderMock.Object
        );

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(@event);
        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _logBufferMock.Verify(x => x.Flush(), Times.Never);
    }

    [Test]
    public async Task GivenEmailSendingFails_WhenHandlingResendEvent_ThenShouldLogErrorAndContinue()
    {
        // Arrange
        List<Outbox> failedEmails =
        [
            new("User One", "user1@example.com", "Subject 1", "Body 1"),
            new("User Two", "user2@example.com", "Subject 2", "Body 2"),
        ];

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedEmails);

        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Send failed"));

        var @event = new ResendErrorEmailIntegrationEvent();
        var handler = new ResendErrorEmailIntegrationEventHandler(
            _loggerMock.Object,
            _logBufferMock.Object,
            _repositoryMock.Object,
            _senderMock.Object
        );

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.Verify(@event);
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
        _logBufferMock.Verify(x => x.Flush(), Times.Once);
    }
}
