using BookWorm.Chat.Memory.Episodic;
using BookWorm.Chat.Memory.Extraction;
using BookWorm.Chat.Memory.Semantic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookWorm.Chat.UnitTests.Memory;

public sealed class MemoryExtractionServiceTests : IDisposable
{
    private const string UserId = "user-a";

    private readonly Mock<IMemoryExtractor> _extractorMock = new();
    private readonly Mock<IEpisodicMemoryStore> _episodicStoreMock = new();
    private readonly Mock<IPreferenceStore> _preferenceStoreMock = new();
    private readonly MemoryExtractionService _sut;

    public MemoryExtractionServiceTests()
    {
        var provider = new ServiceCollection()
            .AddScoped(_ => _preferenceStoreMock.Object)
            .BuildServiceProvider();

        _sut = new(
            new Mock<IMemoryExtractionQueue>().Object,
            _extractorMock.Object,
            _episodicStoreMock.Object,
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<MemoryExtractionService>.Instance
        );
    }

    [Test]
    public async Task GivenExtractedMemories_WhenProcessing_ThenShouldPersistEpisodeAndPreferencesForUser()
    {
        MemoryExtractionRequest request = new(UserId, "I love fantasy", "Here you go");
        PreferenceCandidate preference = new(
            PreferenceKind.Genre,
            "Fantasy",
            PreferenceSentiment.Like
        );
        _extractorMock
            .Setup(e => e.ExtractAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new MemoryExtractionResult("The customer wants fantasy.", [], [preference])
            );

        await _sut.ProcessAsync(request, CancellationToken.None);

        _episodicStoreMock.Verify(
            s =>
                s.AddAsync(
                    UserId,
                    "The customer wants fantasy.",
                    It.IsAny<IReadOnlyCollection<string>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _preferenceStoreMock.Verify(
            s =>
                s.ApplyAsync(
                    UserId,
                    It.Is<IReadOnlyCollection<PreferenceCandidate>>(c => c.Single() == preference),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNothingExtracted_WhenProcessing_ThenShouldNotPersistAnything()
    {
        MemoryExtractionRequest request = new(UserId, "Hi", "Hello!");
        _extractorMock
            .Setup(e => e.ExtractAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MemoryExtractionResult.Empty);

        await _sut.ProcessAsync(request, CancellationToken.None);

        _episodicStoreMock.Verify(
            s =>
                s.AddAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyCollection<string>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        _preferenceStoreMock.Verify(
            s =>
                s.ApplyAsync(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyCollection<PreferenceCandidate>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    public void Dispose()
    {
        _sut.Dispose();
    }
}
