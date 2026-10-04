using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Features.Dashboard;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.UnitTests.Features.Dashboard;

public sealed class GetCatalogDashboardQueryTests
{
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly GetCatalogDashboardHandler _handler;
    private readonly Mock<IBookRepository> _repositoryMock;

    public GetCatalogDashboardQueryTests()
    {
        _repositoryMock = new();
        _cacheMock = new();
        _repositoryMock
            .Setup(repository => repository.CountByCategoryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("Fiction", 30), new("Other", 5)]);
        _cacheMock
            .Setup(cache =>
                cache.GetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<
                        Func<
                            FusionCacheFactoryExecutionContext<CatalogDashboardDto>,
                            CancellationToken,
                            Task<CatalogDashboardDto>
                        >
                    >(),
                    It.IsAny<MaybeValue<CatalogDashboardDto>>(),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<IEnumerable<string>?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(
                (
                    string _,
                    Func<
                        FusionCacheFactoryExecutionContext<CatalogDashboardDto>,
                        CancellationToken,
                        Task<CatalogDashboardDto>
                    > factory,
                    MaybeValue<CatalogDashboardDto> _,
                    FusionCacheEntryOptions? _,
                    IEnumerable<string>? _,
                    CancellationToken token
                ) => new(factory(null!, token))
            );
        _handler = new(_repositoryMock.Object, _cacheMock.Object);
    }

    [Test]
    public async Task GivenCategoryCounts_WhenHandlingDashboardQuery_ThenShouldSumAllTitlesAndMapCategories()
    {
        var result = await _handler.Handle(new(), CancellationToken.None);

        result.TotalBooks.ShouldBe(35);
        result.Categories.ShouldBe([new("Fiction", 30), new("Other", 5)]);
        _repositoryMock.Verify(
            repository => repository.CountByCategoryAsync(CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyRepository_WhenHandlingDashboardQuery_ThenShouldReturnZeroTitlesAndNoCategories()
    {
        _repositoryMock
            .Setup(repository => repository.CountByCategoryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<BookCategoryCount>());

        var result = await _handler.Handle(new(), CancellationToken.None);

        result.TotalBooks.ShouldBe(0);
        result.Categories.ShouldBeEmpty();
    }

    [Test]
    public async Task GivenCachedSummary_WhenHandlingDashboardQuery_ThenShouldSkipRepository()
    {
        var summary = new CatalogDashboardDto(35, [new("Fiction", 30), new("Other", 5)]);
        _cacheMock
            .Setup(cache =>
                cache.GetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<
                        Func<
                            FusionCacheFactoryExecutionContext<CatalogDashboardDto>,
                            CancellationToken,
                            Task<CatalogDashboardDto>
                        >
                    >(),
                    It.IsAny<MaybeValue<CatalogDashboardDto>>(),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<IEnumerable<string>?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(summary);

        var result = await _handler.Handle(new(), CancellationToken.None);

        result.ShouldBe(summary);
        _repositoryMock.Verify(
            repository => repository.CountByCategoryAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenRepositoryFailure_WhenHandlingDashboardQuery_ThenShouldPropagateException()
    {
        var exception = new InvalidOperationException("Catalog summary unavailable.");
        _repositoryMock
            .Setup(repository => repository.CountByCategoryAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var result = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _handler.Handle(new(), CancellationToken.None)
        );

        result.ShouldBe(exception);
    }

    [Test]
    public async Task GivenCancellationToken_WhenHandlingDashboardQuery_ThenShouldPropagateToCacheAndRepository()
    {
        using var cancellation = new CancellationTokenSource();
        var cancellationToken = cancellation.Token;

        await _handler.Handle(new(), cancellationToken);

        _repositoryMock.Verify(
            repository => repository.CountByCategoryAsync(cancellationToken),
            Times.Once
        );
        _cacheMock.Verify(
            cache =>
                cache.GetOrSetAsync(
                    "catalog:dashboard:v1",
                    It.IsAny<
                        Func<
                            FusionCacheFactoryExecutionContext<CatalogDashboardDto>,
                            CancellationToken,
                            Task<CatalogDashboardDto>
                        >
                    >(),
                    It.IsAny<MaybeValue<CatalogDashboardDto>>(),
                    It.Is<FusionCacheEntryOptions?>(options =>
                        options != null && options.Duration == TimeSpan.FromSeconds(15)
                    ),
                    It.IsAny<IEnumerable<string>?>(),
                    cancellationToken
                ),
            Times.Once
        );
    }
}
