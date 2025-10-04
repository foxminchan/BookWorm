var builder = DistributedApplication.CreateBuilder(args);

if (builder.ExecutionContext.IsPublishMode)
{
    builder.ConfigureCors();
    builder.AddAzureContainerAppEnvironment(Components.Azure.ContainerApp).ProvisionAsService();
}

var kcRealmName = builder
    .AddParameter("kc-realm", nameof(BookWorm).ToLowerInvariant(), true)
    .WithDescription(ParameterDescriptions.Keycloak.Realm, true);

var pgUser = builder
    .AddParameter("pg-user", "postgres", true)
    .WithDescription(ParameterDescriptions.Postgres.User, true);

var pgPassword = builder
    .AddParameter("pg-password", true)
    .WithDescription(ParameterDescriptions.Postgres.Password, true)
    .WithGeneratedDefault(
        new()
        {
            MinLength = 16,
            MinUpper = 2,
            MinLower = 2,
            MinNumeric = 2,
            MinSpecial = 2,
        }
    );

var schedulerUserName = builder
    .AddParameter("scheduler-user", "admin", true)
    .WithDescription(ParameterDescriptions.Scheduler.UserName, true);

var schedulerPassword = builder
    .AddParameter("scheduler-password", true)
    .WithDescription(ParameterDescriptions.Scheduler.Password, true)
    .WithGeneratedDefault(
        new()
        {
            MinLength = 16,
            MinUpper = 2,
            MinLower = 2,
            MinNumeric = 2,
            MinSpecial = 2,
        }
    );

var postgres = builder
    .AddAzurePostgresFlexibleServer(Components.Postgres)
    .WithPasswordAuthentication(pgUser, pgPassword)
    .RunAsLocalContainer()
    .ProvisionAsService();

pgUser.WithParentRelationship(postgres);

var redis = builder
    .AddAzureRedis(Components.Redis)
    .WithIconName("Memory")
    .RunAsLocalContainer()
    .ProvisionAsService();

var qdrant = builder
    .AddQdrant(Components.VectorDb)
    .WithDataVolume()
    .WithIconName("DatabaseSearch")
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithLifetime(ContainerLifetime.Persistent);

var queue = builder
    .AddRabbitMQ(Components.Queue)
    .WithManagementPlugin()
    .WithIconName("Pipeline")
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint(Protocols.Tcp, e => e.Port = 5672);

var storage = builder
    .AddAzureStorage(Components.Azure.Storage.Resource)
    .RunAsLocalContainer()
    .ProvisionAsService();

var signalR = builder
    .AddAzureSignalR(Components.Azure.SignalR)
    .RunAsLocalContainer()
    .ProvisionAsService();

var blobStorage = storage.AddBlobContainer(Components.Azure.Storage.BlobContainer);

var ratingDb = postgres.AddDatabase(Components.Database.Rating);
var healthDb = postgres.AddDatabase(Components.Database.Health);
var catalogDb = postgres.AddDatabase(Components.Database.Catalog);
var financeDb = postgres.AddDatabase(Components.Database.Finance);
var orderingDb = postgres.AddDatabase(Components.Database.Ordering);
var schedulerDb = postgres.AddDatabase(Components.Database.Scheduler);
var notificationDb = postgres.AddDatabase(Components.Database.Notification);

await builder.AddOllama(configure =>
{
    configure.AddModel(Components.Ollama.Embedding, Components.Ollama.Google.EmbeddingGemma300M);
    configure.AddModel(
        Components.Ollama.Chat,
        builder.ExecutionContext.IsPublishMode
            ? Components.Ollama.Google.Gemma312B
            : Components.Ollama.Google.Gemma34B
    );
});

IResourceBuilder<IResource> keycloak;

if (builder.ExecutionContext.IsRunMode)
{
    var kcThemeName = builder
        .AddParameter("kc-theme", nameof(BookWorm).ToLowerInvariant(), true)
        .WithDescription(ParameterDescriptions.Keycloak.Theme, true);

    var kcThemeDisplayName = builder
        .AddParameter("kc-theme-display-name", nameof(BookWorm), true)
        .WithDescription(ParameterDescriptions.Keycloak.ThemeDisplayName, true);

    var userDb = postgres.AddDatabase(Components.Database.User);

    keycloak = builder
        .AddKeycloak(Components.KeyCloak)
        .WithIconName("LockClosedRibbon")
        .WithCustomTheme(kcThemeName)
        .WithImagePullPolicy(ImagePullPolicy.Always)
        .WithLifetime(ContainerLifetime.Persistent)
        .WithSampleRealmImport(kcRealmName, kcThemeDisplayName)
        .WithPostgres(userDb);

    kcThemeName.WithParentRelationship(keycloak);
    kcThemeDisplayName.WithParentRelationship(keycloak);
}
else
{
    var keycloakUrl = builder
        .AddParameter("kc-url", true)
        .WithDescription(ParameterDescriptions.Keycloak.Url, true)
        .WithCustomInput(_ =>
            new()
            {
                Name = "KeycloakUrlParameter",
                Label = "Keycloak URL",
                InputType = InputType.Text,
                Value = "https://identity.bookworm.com",
                Description = "Enter your Keycloak server URL here",
            }
        );

    keycloak = builder
        .AddExternalService(Components.KeyCloak, keycloakUrl)
        .WithIconName("LockClosedRibbon")
        .WithHttpHealthCheck("/health/ready");
}

kcRealmName.WithParentRelationship(keycloak);

builder
    .AddOpenTelemetryCollector(
        Components.Observability.Collector,
        settings => settings.ForceNonSecureReceiver = true
    )
    .WithConfig(Path.GetFullPath("Container/otelcollector/config.yaml", builder.AppHostDirectory))
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithAppForwarding();

var catalogApi = builder
    .AddProject<Catalog>(Services.Catalog)
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
    .WithOllama()
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(catalogDb)
    .WaitFor(catalogDb)
    .WithReference(qdrant)
    .WaitFor(qdrant)
    .WithReference(redis)
    .WaitFor(redis)
    .WithIdP(keycloak, kcRealmName)
    .WithReference(blobStorage)
    .WaitFor(blobStorage)
    .WithRoleAssignments(
        storage,
        StorageBuiltInRole.StorageBlobDataContributor,
        StorageBuiltInRole.StorageBlobDataOwner
    );

var mcp = builder
    .AddProject<McpTools>(Services.McpTools)
    .WithReference(catalogApi)
    .WaitFor(catalogApi);

var chatApi = builder
    .AddProject<Chat>(Services.Chatting)
    .WithOllama()
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(mcp)
    .WaitFor(mcp)
    .WithReference(qdrant)
    .WaitFor(qdrant)
    .WithIdP(keycloak, kcRealmName)
    .WithReference(signalR)
    .WaitFor(signalR)
    .WithRoleAssignments(signalR, SignalRBuiltInRole.SignalRContributor);

mcp.WithParentRelationship(chatApi);

var basketApi = builder
    .AddProject<Basket>(Services.Basket)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(catalogApi)
    .WithIdP(keycloak, kcRealmName);

var notificationApi = builder
    .AddProject<Notification>(Services.Notification)
    .WithEmailProvider()
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(notificationDb)
    .WaitFor(notificationDb);

var orderingApi = builder
    .AddProject<Ordering>(Services.Ordering)
    .WithReference(orderingDb)
    .WaitFor(orderingDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(redis)
    .WaitFor(redis)
    .WithIdP(keycloak, kcRealmName)
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithReference(signalR)
    .WaitFor(signalR)
    .WithRoleAssignments(signalR, SignalRBuiltInRole.SignalRContributor)
    .WithHmacSecret();

var ratingApi = builder
    .AddProject<Rating>(Services.Rating)
    .WithOllama()
    .WithReference(ratingDb)
    .WaitFor(ratingDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithIdP(keycloak, kcRealmName)
    .WithReference(chatApi);

chatApi.WithReference(ratingApi).WaitFor(ratingApi);

var financeApi = builder
    .AddProject<Finance>(Services.Finance)
    .WithReference(financeDb)
    .WaitFor(financeDb)
    .WithReference(queue)
    .WaitFor(queue);

var schedulerMigrator = builder
    .AddProject<SchedulerMigrator>(Services.SchedulerMigrator)
    .WithReference(schedulerDb)
    .WaitFor(schedulerDb);

var schedulerApi = builder
    .AddProject<Scheduler>(Services.Scheduler)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(schedulerDb)
    .WaitForCompletion(schedulerMigrator)
    .WithEnvironment("TickerQBasicAuth__Username", schedulerUserName)
    .WithEnvironment("TickerQBasicAuth__Password", schedulerPassword);

schedulerUserName.WithParentRelationship(schedulerApi);
schedulerMigrator.WithParentRelationship(schedulerApi);

var gateway = builder
    .AddApiGatewayProxy()
    .WithService(chatApi)
    .WithService(ratingApi)
    .WithService(orderingApi)
    .WithService(basketApi, true)
    .WithService(catalogApi, true)
    .WithService(keycloak);

builder
    .AddHealthChecksUI()
    .WithStorageProvider(healthDb)
    .WithReference(mcp)
    .WithReference(chatApi)
    .WithReference(ratingApi)
    .WithReference(basketApi)
    .WithReference(catalogApi)
    .WithReference(financeApi)
    .WithReference(orderingApi)
    .WithReference(schedulerApi)
    .WithReference(notificationApi)
    .WithExternalHttpEndpoints();

builder
    .AddScalar(keycloak)
    .WithOpenAPI(mcp)
    .WithOpenAPI(chatApi)
    .WithOpenAPI(basketApi)
    .WithOpenAPI(ratingApi)
    .WithOpenAPI(catalogApi)
    .WithOpenAPI(orderingApi)
    .ExcludeFromManifest();

builder.AddK6(gateway);

builder.Build().Run();
