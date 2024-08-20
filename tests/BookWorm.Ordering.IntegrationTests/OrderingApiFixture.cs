namespace BookWorm.Ordering.IntegrationTests;

public sealed class OrderingApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IHost _app;
    private string? _eventBusConnectionString;
    private string? _orderingDbConnectionString;

    public OrderingApiFixture()
    {
        var options = new DistributedApplicationOptions
        {
            AssemblyName = typeof(OrderingApiFixture).Assembly.FullName, DisableDashboard = true
        };
        var appBuilder = DistributedApplication.CreateBuilder(options);

        OrderingDb = appBuilder.AddPostgres("orderingdb");

        IdentityDb = appBuilder.AddPostgres("identitydb");
        EventBus = appBuilder.AddRabbitMQ("eventbus");
        Redis = appBuilder.AddRedis("redis");

        IdentityApi = appBuilder.AddProject<BookWorm_Identity>("identity-api")
            .WithReference(IdentityDb)
            .WithReference(Redis);

        _app = appBuilder.Build();
    }

    public IResourceBuilder<PostgresServerResource> OrderingDb { get; }

    public IResourceBuilder<RabbitMQServerResource> EventBus { get; }

    public IResourceBuilder<PostgresServerResource> IdentityDb { get; }

    public IResourceBuilder<RedisResource> Redis { get; }

    public IResourceBuilder<ProjectResource> IdentityApi { get; }

    public async Task InitializeAsync()
    {
        await _app.StartAsync();

        _orderingDbConnectionString = await OrderingDb.Resource.GetConnectionStringAsync();
        _eventBusConnectionString = await EventBus.Resource.ConnectionStringExpression.GetValueAsync(default);
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _app.StopAsync();
        if (_app is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            _app.Dispose();
        }
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(cfg =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { $"ConnectionStrings:{OrderingDb.Resource.Name}", _orderingDbConnectionString },
                { "Identity:Url", IdentityApi.GetEndpoint("https").Url },
                { $"ConnectionStrings:{EventBus.Resource.Name}", _eventBusConnectionString }
            });
        });

        builder.ConfigureWebHost(hostBuilder =>
        {
            hostBuilder
                .UseTestServer()
                .ConfigureServices(services => services.RemoveAll<IHostedService>())
                .ConfigureTestServices(services =>
                {
                    services.AddMassTransitTestHarness(x =>
                    {
                        x.AddConsumer<BasketCheckoutFailedIntegrationEventHandler>();
                    });
                });
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IStartupFilter>(new AutoAuthorizeStartupFilter());
        });

        return base.CreateHost(builder);
    }

    private class AutoAuthorizeStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<AutoAuthorizeMiddleware>();
                next(builder);
            };
        }
    }
}
