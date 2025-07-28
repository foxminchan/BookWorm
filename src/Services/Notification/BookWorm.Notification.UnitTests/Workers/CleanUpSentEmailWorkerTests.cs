using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure;
using BookWorm.Notification.UnitTests.Fakers;
using BookWorm.Notification.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BookWorm.Notification.UnitTests.Workers;

public sealed class CleanUpSentEmailWorkerTests : IDisposable
{
    private readonly Mock<INotificationDbContext> _dbContextMock;
    private readonly Mock<ILogger<CleanUpSentEmailWorker>> _loggerMock;
    private readonly ServiceProvider _serviceProvider;
    private readonly CleanUpSentEmailWorker _worker;

    public CleanUpSentEmailWorkerTests()
    {
        _loggerMock = new();
        Mock<GlobalLogBuffer> logBufferMock = new();
        _dbContextMock = new();

        // Create a service collection and add our mock service
        var services = new ServiceCollection();
        services.AddSingleton(_dbContextMock.Object);

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
    public async Task GivenSentEmails_WhenExecuting_ThenShouldRemoveEmailsAndSaveChanges()
    {
        // Arrange
        var sentEmails = TestDataFakers
            .Outbox.Generate(3)
            .Select(x =>
            {
                x.MarkAsSent();
                return x;
            })
            .ToList();

        var mockDbSet = sentEmails.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Outboxes).Returns(mockDbSet.Object);

        var jobContext = Mock.Of<IJobExecutionContext>();

        // Act
        await _worker.Execute(jobContext);

        // Assert
        mockDbSet.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<Outbox>>()), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        VerifyLogMessage(LogLevel.Debug, "Starting cleanup of sent emails", Times.Once());
        VerifyLogMessage(LogLevel.Debug, "Found 3 sent emails to delete", Times.Once());
        VerifyLogMessage(LogLevel.Information, "Successfully cleaned up sent emails", Times.Once());
    }

    [Test]
    public async Task GivenNoSentEmails_WhenExecuting_ThenShouldLogNoEmailsFound()
    {
        // Arrange
        var emptyList = new List<Outbox>();
        var mockDbSet = emptyList.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Outboxes).Returns(mockDbSet.Object);

        var jobContext = Mock.Of<IJobExecutionContext>();

        // Act
        await _worker.Execute(jobContext);

        // Assert
        mockDbSet.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<Outbox>>()), Times.Never);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        VerifyLogMessage(LogLevel.Debug, "Starting cleanup of sent emails", Times.Once());
        VerifyLogMessage(LogLevel.Debug, "No sent emails found for cleanup", Times.Once());
    }

    [Test]
    public async Task GivenDatabaseException_WhenExecuting_ThenShouldLogErrorAndRethrow()
    {
        // Arrange
        var exception = new InvalidOperationException("Database connection failed");
        var jobContext = Mock.Of<IJobExecutionContext>();

        // Create a mock service provider that throws when getting INotificationDbContext
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(x => x.GetService(typeof(INotificationDbContext)))
            .Throws(exception);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

        var worker = new CleanUpSentEmailWorker(
            _loggerMock.Object,
            new Mock<GlobalLogBuffer>().Object,
            scopeFactoryMock.Object
        );

        // Act & Assert
        var thrownException = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await worker.Execute(jobContext)
        );

        thrownException.Message.ShouldBe("Database connection failed");

        VerifyLogMessageWithException(
            LogLevel.Error,
            "Error occurred in job execution",
            exception,
            Times.Once()
        );
    }

    [Test]
    public async Task GivenEntityFrameworkException_WhenQueryingDatabase_ThenShouldLogErrorAndRethrow()
    {
        // Arrange
        var exception = new InvalidOperationException("The connection was not open");
        var jobContext = Mock.Of<IJobExecutionContext>();

        // Create a mock DbSet that throws when accessed during query execution
        var mockDbSet = new Mock<DbSet<Outbox>>();
        mockDbSet.As<IQueryable<Outbox>>().Setup(x => x.Provider).Throws(exception);

        _dbContextMock.Setup(x => x.Outboxes).Returns(mockDbSet.Object);

        // Act & Assert
        var thrownException = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _worker.Execute(jobContext)
        );

        thrownException.Message.ShouldBe("The connection was not open");

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

    [Test]
    public async Task GivenSaveChangesException_WhenExecuting_ThenShouldLogErrorAndRethrow()
    {
        // Arrange
        var exception = new Exception("Failed to save changes");
        var sentEmails = TestDataFakers
            .Outbox.Generate(2)
            .Select(x =>
            {
                x.MarkAsSent();
                return x;
            })
            .ToList();

        var mockDbSet = sentEmails.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Outboxes).Returns(mockDbSet.Object);
        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var jobContext = Mock.Of<IJobExecutionContext>();

        // Act & Assert
        var thrownException = await Should.ThrowAsync<Exception>(async () =>
            await _worker.Execute(jobContext)
        );

        thrownException.Message.ShouldBe("Failed to save changes");

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

    [Test]
    public async Task GivenMixedSentAndUnsentEmails_WhenExecuting_ThenShouldOnlyRemoveSentEmails()
    {
        // Arrange
        var allEmails = TestDataFakers.Outbox.Generate(5);
        var sentEmails = allEmails.Take(3).ToList();

        // Mark only first 3 as sent
        sentEmails.ForEach(x => x.MarkAsSent());

        var mockDbSet = sentEmails.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Outboxes).Returns(mockDbSet.Object);

        var jobContext = Mock.Of<IJobExecutionContext>();

        // Act
        await _worker.Execute(jobContext);

        // Assert
        mockDbSet.Verify(
            x =>
                x.RemoveRange(
                    It.Is<IEnumerable<Outbox>>(emails => VerifyEmailCollection(emails, 3, true))
                ),
            Times.Once
        );

        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenValidCancellationToken_WhenExecuting_ThenShouldPassTokenToDbOperations()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var jobContext = new Mock<IJobExecutionContext>();
        jobContext.Setup(x => x.CancellationToken).Returns(cancellationToken);

        var sentEmails = TestDataFakers
            .Outbox.Generate(1)
            .Select(x =>
            {
                x.MarkAsSent();
                return x;
            })
            .ToList();

        var mockDbSet = sentEmails.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Outboxes).Returns(mockDbSet.Object);

        // Act
        await _worker.Execute(jobContext.Object);

        // Assert
        _dbContextMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenScopeDisposal_WhenExecutingCompletes_ThenShouldDisposeScope()
    {
        // Arrange
        var sentEmails = TestDataFakers
            .Outbox.Generate(1)
            .Select(x =>
            {
                x.MarkAsSent();
                return x;
            })
            .ToList();

        var mockDbSet = sentEmails.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Outboxes).Returns(mockDbSet.Object);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProvider);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

        var worker = new CleanUpSentEmailWorker(
            _loggerMock.Object,
            new Mock<GlobalLogBuffer>().Object,
            scopeFactoryMock.Object
        );
        var jobContext = Mock.Of<IJobExecutionContext>();

        // Act
        await worker.Execute(jobContext);

        // Assert
        scopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
        scopeMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Test]
    [MatrixDataSource]
    public async Task GivenVariousNumberOfSentEmails_WhenExecuting_ThenShouldProcessCorrectly(
        [Matrix(0, 1, 5, 10)] int emailCount
    )
    {
        // Arrange
        var sentEmails = TestDataFakers
            .Outbox.Generate(emailCount)
            .Select(x =>
            {
                x.MarkAsSent();
                return x;
            })
            .ToList();

        var mockDbSet = sentEmails.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Outboxes).Returns(mockDbSet.Object);

        var jobContext = Mock.Of<IJobExecutionContext>();

        // Act
        await _worker.Execute(jobContext);

        // Assert
        if (emailCount > 0)
        {
            mockDbSet.Verify(
                x =>
                    x.RemoveRange(
                        It.Is<IEnumerable<Outbox>>(emails =>
                            VerifyEmailCollection(emails, emailCount, true)
                        )
                    ),
                Times.Once
            );
            _dbContextMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );

            VerifyLogMessage(
                LogLevel.Debug,
                $"Found {emailCount} sent emails to delete",
                Times.Once()
            );
            VerifyLogMessage(
                LogLevel.Information,
                "Successfully cleaned up sent emails",
                Times.Once()
            );
        }
        else
        {
            mockDbSet.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<Outbox>>()), Times.Never);
            _dbContextMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never
            );

            VerifyLogMessage(LogLevel.Debug, "No sent emails found for cleanup", Times.Once());
        }

        // Common verification - should always log starting message
        VerifyLogMessage(LogLevel.Debug, "Starting cleanup of sent emails", Times.Once());
    }

    private static bool VerifyEmailCollection(
        IEnumerable<Outbox> emails,
        int expectedCount,
        bool allShouldBeSent
    )
    {
        var emailList = emails.ToList();
        return emailList.Count == expectedCount
            && (!allShouldBeSent || emailList.All(e => e.IsSent));
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
