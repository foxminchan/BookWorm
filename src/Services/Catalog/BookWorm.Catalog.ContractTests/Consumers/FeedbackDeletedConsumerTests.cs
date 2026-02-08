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

public sealed class FeedbackDeletedConsumerTests
{
    private readonly Guid _bookId;
    private readonly Guid _feedbackId;
    private readonly int _rating;
    private readonly Mock<IBookRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public FeedbackDeletedConsumerTests()
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
    public async Task GivenFeedbackDeletedEvent_WhenPublished_ThenConsumerShouldConsumeItAndRemoveBookRating()
    {
        // Arrange
        var book = new BookFaker().Generate(1)[0];

        _repositoryMock
            .Setup(x => x.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var integrationEvent = new FeedbackDeletedIntegrationEvent(book.Id, _rating, _feedbackId);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<FeedbackDeletedIntegrationEventHandler>())
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(integrationEvent);

            // Assert
            var consumer = harness.GetConsumerHarness<FeedbackDeletedIntegrationEventHandler>();

            // Wait for the consumer to consume the message
            await consumer.Consumed.Any<FeedbackDeletedIntegrationEvent>();

            await SnapshotTestHelper.Verify(new { harness, consumer });

            _repositoryMock.Verify(
                x => x.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()),
                Times.Once
            );

            _unitOfWorkMock.Verify(
                x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Test]
    public async Task GivenFeedbackDeletedEventWithNonExistingBook_WhenHandling_ThenConsumerShouldHandleGracefully()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var integrationEvent = new FeedbackDeletedIntegrationEvent(_bookId, _rating, _feedbackId);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<FeedbackDeletedIntegrationEventHandler>())
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(integrationEvent);

            // Assert
            var consumer = harness.GetConsumerHarness<FeedbackDeletedIntegrationEventHandler>();

            // Wait for the consumer to consume the message
            await consumer.Consumed.Any<FeedbackDeletedIntegrationEvent>();

            await SnapshotTestHelper.Verify(new { harness, consumer });

            _repositoryMock.Verify(
                x => x.GetByIdAsync(_bookId, It.IsAny<CancellationToken>()),
                Times.Once
            );

            _unitOfWorkMock.Verify(
                x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
        finally
        {
            await harness.Stop();
        }
    }
}
