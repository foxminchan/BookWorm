using BookWorm.Chassis.Repository;
using BookWorm.Contracts;
using BookWorm.Rating.Domain.FeedbackAggregator;
using BookWorm.Rating.IntegrationEvents.EventHandlers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Rating.UnitTests.Consumers;

public sealed class BookUpdatedRatingFailedConsumerTests
{
    private readonly Guid _feedbackId;
    private readonly Mock<IFeedbackRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public BookUpdatedRatingFailedConsumerTests()
    {
        _feedbackId = Guid.CreateVersion7();

        _unitOfWorkMock = new();
        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock = new();
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);
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

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<BookUpdatedRatingFailedIntegrationEventHandler>()
            )
            .AddScoped<IFeedbackRepository>(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(integrationEvent);

        // Assert
        var consumerHarness =
            harness.GetConsumerHarness<BookUpdatedRatingFailedIntegrationEventHandler>();

        (
            await consumerHarness.Consumed.Any<BookUpdatedRatingFailedIntegrationEvent>()
        ).ShouldBeTrue();

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_feedbackId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(x => x.Delete(feedback), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);

        await harness.Stop();
    }

    [Test]
    public async Task GivenFailedRatingEventWithNonExistingFeedback_WhenHandling_ThenShouldReturnGracefully()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_feedbackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feedback)null!);

        var integrationEvent = new BookUpdatedRatingFailedIntegrationEvent(_feedbackId);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                x.AddConsumer<BookUpdatedRatingFailedIntegrationEventHandler>()
            )
            .AddScoped<IFeedbackRepository>(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(integrationEvent);

        // Assert
        var consumerHarness =
            harness.GetConsumerHarness<BookUpdatedRatingFailedIntegrationEventHandler>();

        (
            await consumerHarness.Consumed.Any<BookUpdatedRatingFailedIntegrationEvent>()
        ).ShouldBeTrue();

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_feedbackId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(x => x.Delete(It.IsAny<Feedback>()), Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );

        await harness.Stop();
    }
}
