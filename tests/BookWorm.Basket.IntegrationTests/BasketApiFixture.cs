using Aspirant.Hosting;
using BookWorm.Basket.IntegrationEvents.EventHandlers;

namespace BookWorm.Basket.IntegrationTests;

public sealed class BasketApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IHost _app;
    private string? _basketDbConnectionString;
    private string? _eventBusConnectionString;

    public BasketApiFixture()
    {
        var options = new DistributedApplicationOptions
        {
            AssemblyName = typeof(BasketApiFixture).Assembly.FullName, DisableDashboard = true
        };
        var appBuilder = DistributedApplication.CreateBuilder(options);

        IdentityDb = appBuilder.AddPostgres("identitydb");
        EventBus = appBuilder.AddRabbitMQ("eventbus");
        Redis = appBuilder.AddRedis("redis");

        IdentityApi = appBuilder.AddProject<BookWorm_Identity>("identity-api")
            .WithReference(IdentityDb)
            .WithReference(Redis);

        CatalogApi = appBuilder.AddWireMock("catalog-api")
            .RunAsHttp2Service()
            .WithApiMappingBuilder(CatalogApiBuilder.BuildAsync);

        _app = appBuilder.Build();
    }

    public IResourceBuilder<RabbitMQServerResource> EventBus { get; }

    public IResourceBuilder<PostgresServerResource> IdentityDb { get; }

    public IResourceBuilder<RedisResource> Redis { get; }

    public IResourceBuilder<ProjectResource> IdentityApi { get; }

    public IResourceBuilder<WireMockServerResource> CatalogApi { get; }

    public async Task InitializeAsync()
    {
        await _app.StartAsync();

        _basketDbConnectionString = await Redis.Resource.GetConnectionStringAsync();
        _eventBusConnectionString = await EventBus.Resource.ConnectionStringExpression.GetValueAsync(default);

        await Task.Delay(TimeSpan.FromSeconds(5));
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
                { $"ConnectionStrings:{Redis.Resource.Name}", _basketDbConnectionString },
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
                        x.AddConsumer<OrderCreatedIntegrationEventHandler>();
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
