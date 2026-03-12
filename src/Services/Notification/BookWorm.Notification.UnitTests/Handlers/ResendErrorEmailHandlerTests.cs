using BookWorm.Chassis.Repository;
using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Senders;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace BookWorm.Notification.UnitTests.Handlers;

public sealed class ResendErrorEmailHandlerTests
{
    private readonly ResendErrorEmailIntegrationEventHandler _handler;
    private readonly Mock<GlobalLogBuffer> _logBufferMock = new();
    private readonly Mock<ILogger<ResendErrorEmailIntegrationEventHandler>> _loggerMock = new();
    private readonly Mock<IOutboxRepository> _repositoryMock = new();
    private readonly Mock<ISender> _senderMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    public ResendErrorEmailHandlerTests()
    {
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _handler = new(
            _loggerMock.Object,
            _logBufferMock.Object,
            _repositoryMock.Object,
            _senderMock.Object
        );
    }

    [Test]
    public async Task GivenUnsentEmails_WhenConsuming_ThenShouldResendAndMarkAsSent()
    {
        // Arrange
        var email1 = new Outbox("User1", "user1@test.com", "Sub1", "Body1");
        var email2 = new Outbox("User2", "user2@test.com", "Sub2", "Body2");

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([email1, email2]);

        // Act
        await _handler.Handle(new ResendErrorEmailIntegrationEvent(), CancellationToken.None);

        // Assert
        email1.IsSent.ShouldBeTrue();
        email2.IsSent.ShouldBeTrue();
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenNoUnsentEmails_WhenConsuming_ThenShouldReturnEarlyWithoutSending()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _handler.Handle(new ResendErrorEmailIntegrationEvent(), CancellationToken.None);

        // Assert
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GivenSendFailure_WhenConsuming_ThenShouldContinueWithRemainingEmails()
    {
        // Arrange
        var email1 = new Outbox("User1", "user1@test.com", "Sub1", "Body1");
        var email2 = new Outbox("User2", "user2@test.com", "Sub2", "Body2");

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([email1, email2]);

        _senderMock
            .SetupSequence(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Send failed"))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(new ResendErrorEmailIntegrationEvent(), CancellationToken.None);

        // Assert
        email1.IsSent.ShouldBeFalse();
        email2.IsSent.ShouldBeTrue();
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _logBufferMock.Verify(x => x.Flush(), Times.Once);
    }

    [Test]
    public async Task GivenAllSendsFail_WhenConsuming_ThenShouldNotSaveChanges()
    {
        // Arrange
        var email1 = new Outbox("User1", "user1@test.com", "Sub1", "Body1");

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([email1]);

        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Send failed"));

        // Act
        await _handler.Handle(new ResendErrorEmailIntegrationEvent(), CancellationToken.None);

        // Assert
        email1.IsSent.ShouldBeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _logBufferMock.Verify(x => x.Flush(), Times.Once);
    }

    [Test]
    public async Task GivenCancellationRequested_WhenConsuming_ThenShouldThrowOperationCancelled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var email = new Outbox("User1", "user1@test.com", "Sub1", "Body1");

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([email]);

        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            _handler.Handle(new ResendErrorEmailIntegrationEvent(), cts.Token)
        );
    }

    [Test]
    public async Task GivenUnsentEmails_WhenConsuming_ThenShouldQueryWithUnsentOutboxSpec()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _handler.Handle(new ResendErrorEmailIntegrationEvent(), CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenOnlyUnsentEmails_WhenConsuming_ThenShouldProcessAllReturnedEmails()
    {
        // Arrange
        var unsent1 = new Outbox("User1", "u1@test.com", "Sub1", "Body1");
        var unsent2 = new Outbox("User2", "u2@test.com", "Sub2", "Body2");
        var unsent3 = new Outbox("User3", "u3@test.com", "Sub3", "Body3");

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([unsent1, unsent2, unsent3]);

        // Act
        await _handler.Handle(new ResendErrorEmailIntegrationEvent(), CancellationToken.None);

        // Assert
        unsent1.IsSent.ShouldBeTrue();
        unsent2.IsSent.ShouldBeTrue();
        unsent3.IsSent.ShouldBeTrue();
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3)
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenAlreadySentEmailsExcludedBySpec_WhenConsuming_ThenShouldNotReprocessThem()
    {
        // Arrange - simulate that the spec already filtered out sent emails,
        // so the repository returns only unsent ones
        var unsent = new Outbox("Unsent", "unsent@test.com", "Sub", "Body");

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([unsent]);

        // Act
        await _handler.Handle(new ResendErrorEmailIntegrationEvent(), CancellationToken.None);

        // Assert
        unsent.IsSent.ShouldBeTrue();
        _senderMock.Verify(
            x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenSpecReturnsEmailsInOrder_WhenConsuming_ThenShouldProcessInSequence()
    {
        // Arrange - emails ordered by SequenceNumber as the spec dictates
        var first = new Outbox("First", "first@test.com", "Sub1", "Body1") { SequenceNumber = 1 };
        var second = new Outbox("Second", "second@test.com", "Sub2", "Body2")
        {
            SequenceNumber = 2,
        };
        var third = new Outbox("Third", "third@test.com", "Sub3", "Body3") { SequenceNumber = 3 };

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<UnsentOutboxSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([first, second, third]);

        var sendOrder = new List<string>();
        _senderMock
            .Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .Callback<MimeMessage, CancellationToken>(
                (msg, _) => sendOrder.Add(msg.To.Mailboxes.First().Address)
            )
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(new ResendErrorEmailIntegrationEvent(), CancellationToken.None);

        // Assert - processed in the order the spec returned them
        sendOrder.Count.ShouldBe(3);
        sendOrder[0].ShouldBe("first@test.com");
        sendOrder[1].ShouldBe("second@test.com");
        sendOrder[2].ShouldBe("third@test.com");
    }
}
