using BookWorm.Catalog.IntegrationEvents.EventHandlers;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Projects;

namespace BookWorm.Catalog.IntegrationTests;

public sealed class CatalogApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IHost _app;
    private string? _catalogDbConnectionString;
    private string? _eventBusConnectionString;

    public CatalogApiFixture()
    {
        var options = new DistributedApplicationOptions
        {
            AssemblyName = typeof(CatalogApiFixture).Assembly.FullName, DisableDashboard = true
        };
        var appBuilder = DistributedApplication.CreateBuilder(options);

        CatalogDb = appBuilder.AddPostgres("catalogdb")
            .WithImage("ankane/pgvector")
            .WithImageTag("latest");

        IdentityDb = appBuilder.AddPostgres("identitydb");
        EventBus = appBuilder.AddRabbitMQ("eventbus");
        Redis = appBuilder.AddRedis("redis");

        IdentityApi = appBuilder.AddProject<BookWorm_Identity>("identity-api")
            .WithReference(IdentityDb)
            .WithReference(Redis);

        _app = appBuilder.Build();
    }

    public IResourceBuilder<PostgresServerResource> CatalogDb { get; }

    public IResourceBuilder<RabbitMQServerResource> EventBus { get; }

    public IResourceBuilder<PostgresServerResource> IdentityDb { get; }

    public IResourceBuilder<RedisResource> Redis { get; }

    public IResourceBuilder<ProjectResource> IdentityApi { get; }

    public async Task InitializeAsync()
    {
        await _app.StartAsync();

        _catalogDbConnectionString = await CatalogDb.Resource.GetConnectionStringAsync();
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
                { $"ConnectionStrings:{CatalogDb.Resource.Name}", _catalogDbConnectionString },
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
                        x.AddConsumer<FeedbackCreatedIntegrationEventHandler>();
                        x.AddConsumer<FeedbackCreatedIntegrationEventHandler>();
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
