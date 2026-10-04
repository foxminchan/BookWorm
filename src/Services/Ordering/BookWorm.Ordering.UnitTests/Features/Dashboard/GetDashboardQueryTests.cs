using BookWorm.Catalog.Grpc.Services;
using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Features.Dashboard;
using BookWorm.Ordering.Grpc.Services.Book;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Ordering.UnitTests.Features.Dashboard;

public sealed class GetDashboardQueryTests
{
    private readonly Mock<IBookService> _bookServiceMock;
    private readonly Mock<IBuyerRepository> _buyerRepositoryMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly GetDashboardHandler _handler;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly OrderDashboardSummary _orderSummary;

    public GetDashboardQueryTests()
    {
        _orderRepositoryMock = new();
        _buyerRepositoryMock = new();
        _bookServiceMock = new();
        _cacheMock = new();
        _orderSummary = new(
            40,
            250,
            [],
            [
                new(
                    OrderId.From(Guid.Parse("00000000-0000-0000-0000-000000000001")),
                    new(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc),
                    25,
                    Status.Completed
                ),
            ]
        );
        GetDashboardResponse catalogSummary = new()
        {
            TotalBooks = 35,
            Categories =
            {
                new DashboardCategory { Name = "Fiction", Value = 30 },
                new DashboardCategory { Name = "Other", Value = 5 },
            },
        };
        _orderRepositoryMock
            .Setup(repository =>
                repository.GetDashboardAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(_orderSummary);
        _buyerRepositoryMock
            .Setup(repository => repository.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(30);
        _bookServiceMock
            .Setup(service => service.GetDashboardAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(catalogSummary);
        SetupCacheMiss();
        _handler = new(
            _orderRepositoryMock.Object,
            _buyerRepositoryMock.Object,
            _cacheMock.Object,
            _bookServiceMock.Object
        );
    }

    [Test]
    public async Task GivenRepositorySummaries_WhenHandlingDashboardQuery_ThenShouldReturnAllMetrics()
    {
        var result = await _handler.Handle(new(), CancellationToken.None);

        result.TotalOrders.ShouldBe(40);
        result.TotalRevenue.ShouldBe(250);
        result.TotalCustomers.ShouldBe(30);
        result.TotalBooks.ShouldBe(35);
        result.Categories.ShouldBe([new("Fiction", 30), new("Other", 5)]);
        result.RecentOrders.ShouldHaveSingleItem();
        result.RecentOrders[0].Id.ShouldBe((Guid)_orderSummary.RecentOrders[0].Id);
        result.RecentOrders[0].Total.ShouldBe(25);
        result.RecentOrders[0].Status.ShouldBe(nameof(Status.Completed));
        result.RecentOrders[0].Date.Offset.ShouldBe(TimeSpan.Zero);
        _orderRepositoryMock.Verify(
            repository =>
                repository.GetDashboardAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    CancellationToken.None
                ),
            Times.Once
        );
        _buyerRepositoryMock.Verify(
            repository => repository.CountAsync(CancellationToken.None),
            Times.Once
        );
        _bookServiceMock.Verify(
            service => service.GetDashboardAsync(CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenMissingDailyAggregates_WhenHandlingDashboardQuery_ThenShouldFillSevenChronologicalUtcDays()
    {
        _orderRepositoryMock
            .Setup(repository =>
                repository.GetDashboardAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(
                (DateTime start, DateTime end, CancellationToken _) =>
                    Task.FromResult(
                        _orderSummary with
                        {
                            DailyOrders =
                            [
                                new(DateOnly.FromDateTime(start), 2, 10),
                                new(DateOnly.FromDateTime(end.AddDays(-1)), 40, 250),
                            ],
                        }
                    )
            );

        var result = await _handler.Handle(new(), CancellationToken.None);

        var today = DateOnly.FromDateTime(result.UpdatedAt.UtcDateTime);
        result.DailyOrders.Count.ShouldBe(7);
        result
            .DailyOrders.Select(day => day.Date)
            .ShouldBe(Enumerable.Range(-6, 7).Select(today.AddDays));
        result.DailyOrders[0].ShouldBe(new(today.AddDays(-6), 2, 10));
        result.DailyOrders[1].ShouldBe(new(today.AddDays(-5), 0, 0));
        result.DailyOrders[^1].ShouldBe(new(today, 40, 250));
        _orderRepositoryMock.Verify(
            repository =>
                repository.GetDashboardAsync(
                    today.AddDays(-6).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                    today.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                    CancellationToken.None
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyRepositories_WhenHandlingDashboardQuery_ThenShouldReturnZeroMetricsAndSevenZeroDays()
    {
        _orderRepositoryMock
            .Setup(repository =>
                repository.GetDashboardAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new OrderDashboardSummary(0, 0, [], []));
        _buyerRepositoryMock
            .Setup(repository => repository.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _bookServiceMock
            .Setup(service => service.GetDashboardAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetDashboardResponse());

        var result = await _handler.Handle(new(), CancellationToken.None);

        result.TotalOrders.ShouldBe(0);
        result.TotalRevenue.ShouldBe(0);
        result.TotalCustomers.ShouldBe(0);
        result.TotalBooks.ShouldBe(0);
        result.Categories.ShouldBeEmpty();
        result.RecentOrders.ShouldBeEmpty();
        result.DailyOrders.Count.ShouldBe(7);
        result.DailyOrders.ShouldAllBe(day => day.Orders == 0 && day.Revenue == 0);
    }

    [Test]
    public async Task GivenCachedDashboard_WhenHandlingDashboardQuery_ThenShouldSkipRepositoriesAndCatalog()
    {
        var snapshot = new DashboardDto(
            40,
            250,
            30,
            35,
            [],
            [],
            [],
            new(2026, 10, 4, 0, 0, 0, TimeSpan.Zero)
        );
        _cacheMock
            .Setup(cache =>
                cache.GetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<
                        Func<
                            FusionCacheFactoryExecutionContext<DashboardDto>,
                            CancellationToken,
                            Task<DashboardDto>
                        >
                    >(),
                    It.IsAny<MaybeValue<DashboardDto>>(),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<IEnumerable<string>?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(snapshot);

        var result = await _handler.Handle(new(), CancellationToken.None);

        result.ShouldBe(snapshot);
        _orderRepositoryMock.Verify(
            repository =>
                repository.GetDashboardAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        _buyerRepositoryMock.Verify(
            repository => repository.CountAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
        _bookServiceMock.Verify(
            service => service.GetDashboardAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenOrderRepositoryFailure_WhenHandlingDashboardQuery_ThenShouldPropagateException()
    {
        var exception = new InvalidOperationException("Order summary unavailable.");
        _orderRepositoryMock
            .Setup(repository =>
                repository.GetDashboardAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(exception);

        var result = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _handler.Handle(new(), CancellationToken.None)
        );

        result.ShouldBe(exception);
        _buyerRepositoryMock.Verify(
            repository => repository.CountAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenCatalogFailure_WhenHandlingDashboardQuery_ThenShouldPropagateExceptionWithoutQueryingOrders()
    {
        var exception = new InvalidOperationException("Catalog unavailable.");
        _bookServiceMock
            .Setup(service => service.GetDashboardAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var result = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _handler.Handle(new(), CancellationToken.None)
        );

        result.ShouldBe(exception);
        _orderRepositoryMock.Verify(
            repository =>
                repository.GetDashboardAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Test]
    public async Task GivenCancellationToken_WhenHandlingDashboardQuery_ThenShouldPropagateToAllDependencies()
    {
        using var cancellation = new CancellationTokenSource();
        var cancellationToken = cancellation.Token;

        await _handler.Handle(new(), cancellationToken);

        _orderRepositoryMock.Verify(
            repository =>
                repository.GetDashboardAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    cancellationToken
                ),
            Times.Once
        );
        _buyerRepositoryMock.Verify(
            repository => repository.CountAsync(cancellationToken),
            Times.Once
        );
        _bookServiceMock.Verify(
            service => service.GetDashboardAsync(cancellationToken),
            Times.Once
        );
        _cacheMock.Verify(
            cache =>
                cache.GetOrSetAsync(
                    "ordering:dashboard:v1",
                    It.IsAny<
                        Func<
                            FusionCacheFactoryExecutionContext<DashboardDto>,
                            CancellationToken,
                            Task<DashboardDto>
                        >
                    >(),
                    It.IsAny<MaybeValue<DashboardDto>>(),
                    It.Is<FusionCacheEntryOptions?>(options =>
                        options != null && options.Duration == TimeSpan.FromSeconds(15)
                    ),
                    It.IsAny<IEnumerable<string>?>(),
                    cancellationToken
                ),
            Times.Once
        );
    }

    private void SetupCacheMiss()
    {
        _cacheMock
            .Setup(cache =>
                cache.GetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<
                        Func<
                            FusionCacheFactoryExecutionContext<DashboardDto>,
                            CancellationToken,
                            Task<DashboardDto>
                        >
                    >(),
                    It.IsAny<MaybeValue<DashboardDto>>(),
                    It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<IEnumerable<string>?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(
                (
                    string _,
                    Func<
                        FusionCacheFactoryExecutionContext<DashboardDto>,
                        CancellationToken,
                        Task<DashboardDto>
                    > factory,
                    MaybeValue<DashboardDto> _,
                    FusionCacheEntryOptions? _,
                    IEnumerable<string>? _,
                    CancellationToken token
                ) => new(factory(null!, token))
            );
    }
}
