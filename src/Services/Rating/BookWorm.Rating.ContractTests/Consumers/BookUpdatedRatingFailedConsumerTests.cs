using BookWorm.Chassis.EventBus.Serialization;
using BookWorm.Chassis.Repository;
using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Rating.Domain.FeedbackAggregator;
using BookWorm.Rating.IntegrationEvents.EventHandlers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Rating.ContractTests.Consumers;

public sealed class BookUpdatedRatingFailedConsumerTests
{
    private Guid _feedbackId;
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;
    private Mock<IFeedbackRepository> _repositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;

    [Before(Test)]
    public async Task SetUpAsync()
    {
        _feedbackId = Guid.CreateVersion7();

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
                x.AddConsumer<BookUpdatedRatingFailedIntegrationEventHandler>();
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
    public async Task GivenValidFailedRatingEvent_WhenHandling_ThenShouldDeleteFeedback()
    {
        // Arrange
        var feedback = new Feedback(_feedbackId, "John", "Doe", "Great book!", 4);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(_feedbackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedback);

        var integrationEvent = new BookUpdatedRatingFailedIntegrationEvent(_feedbackId);

        // Act
        await _harness.Bus.Publish(integrationEvent);

        // Assert
        var consumer =
            _harness.GetConsumerHarness<BookUpdatedRatingFailedIntegrationEventHandler>();
        await consumer.Consumed.Any<BookUpdatedRatingFailedIntegrationEvent>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_feedbackId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(x => x.Delete(feedback), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenFailedRatingEventWithNonExistingFeedback_WhenHandling_ThenShouldReturnGracefully()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_feedbackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feedback?)null);

        var integrationEvent = new BookUpdatedRatingFailedIntegrationEvent(_feedbackId);

        // Act
        await _harness.Bus.Publish(integrationEvent);

        // Assert
        var consumer =
            _harness.GetConsumerHarness<BookUpdatedRatingFailedIntegrationEventHandler>();
        await consumer.Consumed.Any<BookUpdatedRatingFailedIntegrationEvent>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_feedbackId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(x => x.Delete(It.IsAny<Feedback>()), Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
