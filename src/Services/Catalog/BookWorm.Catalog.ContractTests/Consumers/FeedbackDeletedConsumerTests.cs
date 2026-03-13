using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.IntegrationEvents.EventHandlers;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.Chassis.EventBus.Serialization;
using BookWorm.Chassis.Repository;
using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Catalog.ContractTests.Consumers;

public sealed class FeedbackDeletedConsumerTests
{
    private readonly int _rating = 4;
    private Book _book = null!;
    private Guid _bookId;
    private Guid _feedbackId;
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;
    private Mock<IBookRepository> _repositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;

    [Before(Test)]
    public async Task SetUpAsync()
    {
        _bookId = Guid.CreateVersion7();
        _feedbackId = Guid.CreateVersion7();
        _book = new BookFaker().Generate(1)[0];

        _unitOfWorkMock = new();
        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock = new();
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _provider = new ServiceCollection()
            .AddTelemetryListener()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<FeedbackDeletedIntegrationEventHandler>();
                x.UsingInMemory(
                    (context, cfg) =>
                    {
                        cfg.UseCloudEvents();
                        cfg.ConfigureEndpoints(context);
                    }
                );
            })
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        _harness = await _provider.StartTestHarness();
    }

    [After(Test)]
    public async Task TearDownAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Test]
    public async Task GivenFeedbackDeletedEvent_WhenPublished_ThenConsumerShouldConsumeItAndRemoveBookRating()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_book);

        var integrationEvent = new FeedbackDeletedIntegrationEvent(_book.Id, _rating, _feedbackId);

        // Act
        await _harness.Bus.Publish(integrationEvent);

        // Assert
        var consumer = _harness.GetConsumerHarness<FeedbackDeletedIntegrationEventHandler>();
        await consumer.Consumed.Any<FeedbackDeletedIntegrationEvent>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_book.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenFeedbackDeletedEventWithNonExistingBook_WhenHandling_ThenConsumerShouldHandleGracefully()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var integrationEvent = new FeedbackDeletedIntegrationEvent(_bookId, _rating, _feedbackId);

        // Act
        await _harness.Bus.Publish(integrationEvent);

        // Assert
        var consumer = _harness.GetConsumerHarness<FeedbackDeletedIntegrationEventHandler>();
        await consumer.Consumed.Any<FeedbackDeletedIntegrationEvent>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_bookId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
