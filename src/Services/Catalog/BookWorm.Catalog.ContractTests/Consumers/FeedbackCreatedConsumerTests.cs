using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.IntegrationEvents.EventHandlers;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.Chassis.Repository;
using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Catalog.ContractTests.Consumers;

public sealed class FeedbackCreatedConsumerTests : SnapshotTestBase
{
    private readonly Guid _feedbackId;
    private readonly int _rating;
    private readonly Mock<IBookRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public FeedbackCreatedConsumerTests()
    {
        Guid.CreateVersion7();
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
    public async Task GivenFeedbackCreatedEvent_WhenPublished_ThenConsumerShouldConsumeItAndUpdateBookRating()
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

        var consumedMessage = consumerHarness
            .Consumed.Select<FeedbackCreatedIntegrationEvent>()
            .First();

        // Verify the consumed event contract structure to ensure consumer compatibility
        await VerifySnapshot(
            new
            {
                EventType = nameof(FeedbackCreatedIntegrationEvent),
                Properties = new
                {
                    consumedMessage.Context.Message.BookId,
                    consumedMessage.Context.Message.Rating,
                    consumedMessage.Context.Message.FeedbackId,
                },
                Schema = new
                {
                    BookIdType = consumedMessage.Context.Message.BookId.GetType().Name,
                    RatingType = consumedMessage.Context.Message.Rating.GetType().Name,
                    FeedbackIdType = consumedMessage.Context.Message.FeedbackId.GetType().Name,
                    HasId = consumedMessage.Context.Message.Id != Guid.Empty,
                    HasCreationDate = consumedMessage.Context.Message.CreationDate != default,
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
}
