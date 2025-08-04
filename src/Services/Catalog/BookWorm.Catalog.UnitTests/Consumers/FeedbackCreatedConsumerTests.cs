using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.IntegrationEvents.EventHandlers;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.Chassis.Repository;
using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Catalog.UnitTests.Consumers;

public sealed class FeedbackCreatedConsumerTests : SnapshotTestBase
{
    private readonly Guid _bookId;
    private readonly Guid _feedbackId;
    private readonly int _rating;
    private readonly Mock<IBookRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public FeedbackCreatedConsumerTests()
    {
        _bookId = Guid.CreateVersion7();
        _feedbackId = Guid.CreateVersion7();
        _rating = 4;

        _unitOfWorkMock = new();
        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock = new();
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task GivenValidFeedbackCreatedEvent_WhenHandling_ThenShouldUpdateBookRating()
    {
        // Arrange
        var book = new BookFaker().Generate(1)[0];

        _repositoryMock
            .Setup(x => x.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var integrationEvent = new FeedbackCreatedIntegrationEvent(book.Id, _rating, _feedbackId);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<FeedbackCreatedIntegrationEventHandler>())
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(integrationEvent);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<FeedbackCreatedIntegrationEventHandler>();

        (await consumerHarness.Consumed.Any<FeedbackCreatedIntegrationEvent>()).ShouldBeTrue();

        (await harness.Published.Any<BookUpdatedRatingFailedIntegrationEvent>()).ShouldBeFalse();

        // Verify the input event structure to ensure consumer contract compatibility
        await VerifySnapshot(
            new
            {
                EventType = nameof(FeedbackCreatedIntegrationEvent),
                Properties = new
                {
                    integrationEvent.BookId,
                    integrationEvent.Rating,
                    integrationEvent.FeedbackId,
                },
                Schema = new
                {
                    BookIdType = integrationEvent.BookId.GetType().Name,
                    RatingType = integrationEvent.Rating.GetType().Name,
                    FeedbackIdType = integrationEvent.FeedbackId.GetType().Name,
                    HasId = integrationEvent.Id != Guid.Empty,
                    HasCreationDate = integrationEvent.CreationDate != default,
                    IsIntegrationEvent = true,
                },
            }
        );

        _repositoryMock.Verify(
            x => x.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);

        await harness.Stop();
    }

    [Test]
    public async Task GivenFeedbackCreatedEventWithNonExistingBook_WhenHandling_ThenShouldPublishFailedEvent()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var integrationEvent = new FeedbackCreatedIntegrationEvent(_bookId, _rating, _feedbackId);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<FeedbackCreatedIntegrationEventHandler>())
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(integrationEvent);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<FeedbackCreatedIntegrationEventHandler>();

        (await consumerHarness.Consumed.Any<FeedbackCreatedIntegrationEvent>()).ShouldBeTrue();
        (await harness.Published.Any<BookUpdatedRatingFailedIntegrationEvent>()).ShouldBeTrue();

        var publishedMessage = harness
            .Published.Select<BookUpdatedRatingFailedIntegrationEvent>()
            .First();
        publishedMessage.Context.Message.FeedbackId.ShouldBe(_feedbackId);

        // Verify contract structure and properties for the failed event
        await VerifySnapshot(
            new
            {
                EventType = nameof(BookUpdatedRatingFailedIntegrationEvent),
                Properties = new { publishedMessage.Context.Message.FeedbackId },
                Schema = new
                {
                    FeedbackIdType = publishedMessage.Context.Message.FeedbackId.GetType().Name,
                    HasId = publishedMessage.Context.Message.Id != Guid.Empty,
                    HasCreationDate = publishedMessage.Context.Message.CreationDate != default,
                },
            }
        );

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_bookId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);

        await harness.Stop();
    }

    [Test]
    public async Task GivenFeedbackCreatedIntegrationEvent_ThenShouldVerifyInputContract()
    {
        // Arrange
        var integrationEvent = new FeedbackCreatedIntegrationEvent(_bookId, _rating, _feedbackId);

        // Verify the input event structure to ensure consumer contract compatibility
        await VerifySnapshot(
            new
            {
                EventType = nameof(FeedbackCreatedIntegrationEvent),
                Properties = new
                {
                    integrationEvent.BookId,
                    integrationEvent.Rating,
                    integrationEvent.FeedbackId,
                },
                Schema = new
                {
                    BookIdType = integrationEvent.BookId.GetType().Name,
                    RatingType = integrationEvent.Rating.GetType().Name,
                    FeedbackIdType = integrationEvent.FeedbackId.GetType().Name,
                    HasId = integrationEvent.Id != Guid.Empty,
                    HasCreationDate = integrationEvent.CreationDate != default,
                    IsIntegrationEvent = true,
                },
            }
        );
    }
}
