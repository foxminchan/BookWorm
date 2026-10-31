using System.Linq.Expressions;
using BookWorm.Chat.Memory;
using BookWorm.Chat.Memory.Episodic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace BookWorm.Chat.UnitTests.Memory;

public sealed class QdrantEpisodicMemoryStoreTests
{
    private const string UserA = "user-a";
    private const string UserB = "user-b";

    private static readonly DateTimeOffset _now = new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly Mock<VectorStoreCollection<Guid, EpisodicMemory>> _collectionMock = new();
    private readonly Mock<IEmbeddingGenerator<string, Embedding<float>>> _embeddingMock = new();
    private readonly UserMemoryOptions _options = new()
    {
        EpisodeRetention = TimeSpan.FromDays(90),
        MinEpisodeRelevance = 0.3,
    };
    private readonly QdrantEpisodicMemoryStore _sut;

    public QdrantEpisodicMemoryStoreTests()
    {
        _embeddingMock
            .Setup(e =>
                e.GenerateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<EmbeddingGenerationOptions?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new GeneratedEmbeddings<Embedding<float>>([new(new float[] { 1f, 0f, 0f })])
            );

        _sut = new(
            _collectionMock.Object,
            _embeddingMock.Object,
            _options,
            new TestTimeProvider(_now)
        );
    }

    [Test]
    public async Task GivenUser_WhenSearching_ThenFilterShouldOnlyMatchThatUsersUnexpiredEpisodes()
    {
        VectorSearchOptions<EpisodicMemory>? capturedOptions = null;
        SetupSearch([], options => capturedOptions = options);

        await _sut.SearchAsync(UserA, "space opera", 3);

        capturedOptions.ShouldNotBeNull();
        capturedOptions.Filter.ShouldNotBeNull();
        var filter = capturedOptions.Filter.Compile();

        filter(CreateEpisode(UserA, _now.AddDays(-1))).ShouldBeTrue();
        filter(CreateEpisode(UserB, _now.AddDays(-1))).ShouldBeFalse();
        filter(CreateEpisode(UserA, _now.AddDays(-91))).ShouldBeFalse();
    }

    [Test]
    public async Task GivenResultsFromAnotherUser_WhenSearching_ThenShouldNeverReturnThem()
    {
        var own = CreateEpisode(UserA, _now.AddDays(-1));
        var foreign = CreateEpisode(UserB, _now.AddDays(-1));
        SetupSearch([new(foreign, 0.95), new(own, 0.9)]);

        var result = await _sut.SearchAsync(UserA, "space opera", 3);

        result.Select(e => e.Id).ShouldBe([own.Id]);
    }

    [Test]
    public async Task GivenResults_WhenSearching_ThenShouldDropIrrelevantAndRankRecentFirstOnTies()
    {
        var older = CreateEpisode(UserA, _now.AddDays(-60));
        var recent = CreateEpisode(UserA, _now.AddDays(-1));
        var irrelevant = CreateEpisode(UserA, _now.AddDays(-1));
        SetupSearch([new(older, 0.8), new(recent, 0.8), new(irrelevant, 0.1)]);

        var result = await _sut.SearchAsync(UserA, "space opera", 3);

        result.Select(e => e.Id).ShouldBe([recent.Id, older.Id]);
    }

    [Test]
    public async Task GivenEmptyQuery_WhenSearching_ThenShouldReturnNothingWithoutQuerying()
    {
        var result = await _sut.SearchAsync(UserA, "   ", 3);

        result.ShouldBeEmpty();
        _embeddingMock.Verify(
            e =>
                e.GenerateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<EmbeddingGenerationOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Test]
    public async Task GivenSummary_WhenAdding_ThenShouldStoreUserScopedEpisodeWithRetentionExpiry()
    {
        EpisodicMemory? stored = null;
        _collectionMock
            .Setup(c => c.UpsertAsync(It.IsAny<EpisodicMemory>(), It.IsAny<CancellationToken>()))
            .Callback<EpisodicMemory, CancellationToken>((record, _) => stored = record)
            .Returns(Task.CompletedTask);

        await _sut.AddAsync(UserA, "The customer bought Dune.", ["book-1"]);

        stored.ShouldNotBeNull();
        stored.UserId.ShouldBe(UserA);
        stored.Summary.ShouldBe("The customer bought Dune.");
        stored.BookIds.ShouldBe(["book-1"]);
        stored.CreatedAt.ShouldBe(_now);
        stored.ExpiresAt.ShouldBe(_now.AddDays(90));
    }

    [Test]
    public async Task GivenEpisodeOwnedByAnotherUser_WhenDeleting_ThenShouldNotDeleteIt()
    {
        var foreign = CreateEpisode(UserB, _now.AddDays(-1));
        _collectionMock
            .Setup(c =>
                c.GetAsync(
                    foreign.Id,
                    It.IsAny<RecordRetrievalOptions?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(foreign);

        var deleted = await _sut.DeleteAsync(UserA, foreign.Id);

        deleted.ShouldBeFalse();
        _collectionMock.Verify(
            c => c.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenUser_WhenDeletingAll_ThenFilterShouldOnlyMatchThatUser()
    {
        Expression<Func<EpisodicMemory, bool>>? capturedFilter = null;
        _collectionMock
            .Setup(c =>
                c.GetAsync(
                    It.IsAny<Expression<Func<EpisodicMemory, bool>>>(),
                    It.IsAny<int>(),
                    It.IsAny<FilteredRecordRetrievalOptions<EpisodicMemory>?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<
                Expression<Func<EpisodicMemory, bool>>,
                int,
                FilteredRecordRetrievalOptions<EpisodicMemory>?,
                CancellationToken
            >((filter, _, _, _) => capturedFilter = filter)
            .Returns(AsyncEnumerable.Empty<EpisodicMemory>());

        await _sut.DeleteAllAsync(UserA);

        capturedFilter.ShouldNotBeNull();
        var filter = capturedFilter.Compile();
        filter(CreateEpisode(UserA, _now)).ShouldBeTrue();
        filter(CreateEpisode(UserB, _now)).ShouldBeFalse();
    }

    private void SetupSearch(
        IEnumerable<VectorSearchResult<EpisodicMemory>> results,
        Action<VectorSearchOptions<EpisodicMemory>?>? onSearch = null
    )
    {
        _collectionMock
            .Setup(c =>
                c.SearchAsync(
                    It.IsAny<ReadOnlyMemory<float>>(),
                    It.IsAny<int>(),
                    It.IsAny<VectorSearchOptions<EpisodicMemory>?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<
                ReadOnlyMemory<float>,
                int,
                VectorSearchOptions<EpisodicMemory>?,
                CancellationToken
            >((_, _, options, _) => onSearch?.Invoke(options))
            .Returns(results.ToAsyncEnumerable());
    }

    private EpisodicMemory CreateEpisode(string userId, DateTimeOffset createdAt)
    {
        return new()
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Summary = "The customer asked about space opera.",
            CreatedAt = createdAt,
            ExpiresAt = createdAt.Add(_options.EpisodeRetention),
        };
    }
}
