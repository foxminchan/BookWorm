using BookWorm.Chat.Memory;
using BookWorm.Chat.Memory.Episodic;
using BookWorm.Chat.Memory.Semantic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookWorm.Chat.UnitTests.Memory;

public sealed class UserMemoryServiceTests
{
    private const string UserId = "user-a";

    private readonly Mock<IPreferenceStore> _preferenceStoreMock = new();
    private readonly Mock<IEpisodicMemoryStore> _episodicStoreMock = new();
    private readonly UserMemoryOptions _options = new() { MaxPreferences = 4, MaxEpisodes = 2 };
    private readonly UserMemoryService _sut;

    public UserMemoryServiceTests()
    {
        var provider = new ServiceCollection()
            .AddScoped(_ => _preferenceStoreMock.Object)
            .BuildServiceProvider();

        _sut = new(
            provider.GetRequiredService<IServiceScopeFactory>(),
            _episodicStoreMock.Object,
            _options,
            NullLogger<UserMemoryService>.Instance
        );
    }

    [Test]
    public async Task GivenNoMemories_WhenRecalling_ThenShouldReturnEmptySnapshot()
    {
        _preferenceStoreMock
            .Setup(s => s.GetRelevantAsync(UserId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _episodicStoreMock
            .Setup(s =>
                s.SearchAsync(
                    UserId,
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([]);

        var result = await _sut.RecallAsync(UserId, "fantasy");

        result.IsEmpty.ShouldBeTrue();
    }

    [Test]
    public async Task GivenStoredMemories_WhenRecalling_ThenShouldQueryBothStoresForTheUserWithLimits()
    {
        RememberedPreference preference = new(
            Guid.CreateVersion7(),
            PreferenceKind.Author,
            "Ursula K. Le Guin",
            PreferenceSentiment.Like,
            0.7,
            2,
            DateTime.UtcNow
        );
        RememberedEpisode episode = new(
            Guid.CreateVersion7(),
            "The customer asked for Earthsea-like fantasy.",
            [],
            DateTimeOffset.UtcNow,
            0.82
        );

        _preferenceStoreMock
            .Setup(s => s.GetRelevantAsync(UserId, 4, It.IsAny<CancellationToken>()))
            .ReturnsAsync([preference]);
        _episodicStoreMock
            .Setup(s => s.SearchAsync(UserId, "fantasy", 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync([episode]);

        var result = await _sut.RecallAsync(UserId, "fantasy");

        result.Preferences.ShouldBe([preference]);
        result.Episodes.ShouldBe([episode]);
    }

    [Test]
    public async Task GivenNoQuery_WhenRecalling_ThenShouldOnlyRecallPreferences()
    {
        _preferenceStoreMock
            .Setup(s => s.GetRelevantAsync(UserId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.RecallAsync(UserId, null);

        result.IsEmpty.ShouldBeTrue();
        _episodicStoreMock.Verify(
            s =>
                s.SearchAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Test]
    public async Task GivenFailingEpisodicStore_WhenRecalling_ThenShouldStillReturnPreferences()
    {
        RememberedPreference preference = new(
            Guid.CreateVersion7(),
            PreferenceKind.Genre,
            "Mystery",
            PreferenceSentiment.Like,
            0.6,
            1,
            DateTime.UtcNow
        );

        _preferenceStoreMock
            .Setup(s => s.GetRelevantAsync(UserId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([preference]);
        _episodicStoreMock
            .Setup(s =>
                s.SearchAsync(
                    UserId,
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new InvalidOperationException("Qdrant unavailable"));

        var result = await _sut.RecallAsync(UserId, "mystery");

        result.Preferences.ShouldBe([preference]);
        result.Episodes.ShouldBeEmpty();
    }

    [Test]
    public async Task GivenUser_WhenForgettingAll_ThenShouldDeleteFromBothStoresForThatUserOnly()
    {
        await _sut.ForgetAllAsync(UserId);

        _preferenceStoreMock.Verify(
            s => s.DeleteAllAsync(UserId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _episodicStoreMock.Verify(
            s => s.DeleteAllAsync(UserId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task GivenMissingUserId_WhenRecalling_ThenShouldThrow(string userId)
    {
        await Should.ThrowAsync<ArgumentException>(() => _sut.RecallAsync(userId, "query"));
    }
}
