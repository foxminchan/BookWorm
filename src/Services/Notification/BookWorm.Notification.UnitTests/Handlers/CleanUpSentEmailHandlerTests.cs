using BookWorm.Chassis.Repository;
using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.IntegrationEvents.EventHandlers;
using MassTransit;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Logging;

namespace BookWorm.Notification.UnitTests.Handlers;

public sealed class CleanUpSentEmailHandlerTests
{
    private readonly Mock<ConsumeContext<CleanUpSentEmailIntegrationEvent>> _contextMock = new();
    private readonly CleanUpSentEmailIntegrationEventHandler _handler;
    private readonly Mock<GlobalLogBuffer> _logBufferMock = new();
    private readonly Mock<ILogger<CleanUpSentEmailIntegrationEventHandler>> _loggerMock = new();
    private readonly Mock<IOutboxRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    public CleanUpSentEmailHandlerTests()
    {
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _handler = new(_loggerMock.Object, _logBufferMock.Object, _repositoryMock.Object);
    }

    [Test]
    public async Task GivenSentEmails_WhenConsuming_ThenShouldDeleteAndSaveChanges()
    {
        // Arrange
        var email1 = new Outbox("User1", "user1@test.com", "Sub1", "Body1");
        email1.MarkAsSent();
        var email2 = new Outbox("User2", "user2@test.com", "Sub2", "Body2");
        email2.MarkAsSent();

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([email1, email2]);

        // Act
        await _handler.Consume(_contextMock.Object);

        // Assert
        _repositoryMock.Verify(
            x => x.BulkDelete(It.Is<IEnumerable<Outbox>>(e => e.Count() == 2)),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenNoSentEmails_WhenConsuming_ThenShouldNotDeleteOrSave()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _handler.Consume(_contextMock.Object);

        // Assert
        _repositoryMock.Verify(x => x.BulkDelete(It.IsAny<IEnumerable<Outbox>>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GivenRepositoryThrows_WhenConsuming_ThenShouldFlushAndThrowInvalidOperationException()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<OutboxFilterSpec>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _handler.Consume(_contextMock.Object)
        );

        exception.Message.ShouldBe("Failed to clean up sent emails");
        _logBufferMock.Verify(x => x.Flush(), Times.Once);
    }
}
