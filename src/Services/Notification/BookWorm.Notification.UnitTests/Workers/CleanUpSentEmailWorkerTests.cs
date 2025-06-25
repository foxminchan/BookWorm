using BookWorm.Notification.Infrastructure.Table;
using BookWorm.Notification.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Logging;
using Quartz;

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
    public async Task GivenCleanupRequest_WhenExecuting_ThenShouldCallBulkDeleteAsync()
    {
        // Arrange
        var jobContext = Mock.Of<IJobExecutionContext>();

        // Act
        await _worker.Execute(jobContext);

        // Assert
        _tableServiceMock.Verify(
            x => x.BulkDeleteAsync(TablePartition.Processed, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("Starting cleanup of sent emails")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenException_WhenExecuting_ThenShouldLogErrorAndRethrow()
    {
        // Arrange
        var exception = new Exception("Table service error");
        var jobContext = Mock.Of<IJobExecutionContext>();

        _tableServiceMock
            .Setup(x => x.BulkDeleteAsync(TablePartition.Processed, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Should.ThrowAsync<Exception>(async () =>
            await _worker.Execute(jobContext)
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

    [Test]
    public async Task GivenSuccessfulExecution_WhenExecuting_ThenShouldLogDebugMessage()
    {
        // Arrange
        var jobContext = Mock.Of<IJobExecutionContext>();

        // Act
        await _worker.Execute(jobContext);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => o.ToString()!.Contains("Starting cleanup of sent emails")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}
