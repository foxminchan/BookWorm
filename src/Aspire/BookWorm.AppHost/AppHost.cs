var builder = DistributedApplication.CreateBuilder(args);

if (builder.ExecutionContext.IsPublishMode)
{
    builder.ConfigureCors();
    builder.AddAzureContainerAppEnvironment(Components.Azure.ContainerApp).ProvisionAsService();
}

var kcRealmName = builder
    .AddParameter("kc-realm", nameof(BookWorm).ToLowerInvariant(), true)
    .WithDescription(ParameterDescriptions.Keycloak.Realm, true);

var kcThemeName = builder
    .AddParameter("kc-theme", nameof(BookWorm).ToLowerInvariant(), true)
    .WithDescription(ParameterDescriptions.Keycloak.Theme, true);

var kcThemeDisplayName = builder
    .AddParameter("kc-theme-display-name", nameof(BookWorm), true)
    .WithDescription(ParameterDescriptions.Keycloak.ThemeDisplayName, true);

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

ReferenceExpression? pgEndpoint = null;

var postgres = builder
    .AddAzurePostgresFlexibleServer(Components.Postgres)
    .WithPasswordAuthentication(pgUser, pgPassword)
    .RunAsLocalContainer(cfg =>
        pgEndpoint = ReferenceExpression.Create(
            $"{cfg.Resource.PrimaryEndpoint.Property(EndpointProperty.Host)}:{cfg.Resource.PrimaryEndpoint.Property(EndpointProperty.Port)}"
        )
    )
    .ProvisionAsService();

pgUser.WithParentRelationship(postgres);
pgPassword.WithParentRelationship(postgres);

pgEndpoint ??= ReferenceExpression.Create($"{postgres.GetOutput("hostname")}");

var redis = builder.AddAzureRedis(Components.Redis).RunAsLocalContainer().ProvisionAsService();

var qdrant = builder
    .AddQdrant(Components.VectorDb)
    .WithDataVolume()
    .WithImageTag("v1.15.1")
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithLifetime(ContainerLifetime.Persistent);

var queue = builder
    .AddRabbitMQ(Components.Queue)
    .WithManagementPlugin()
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

var chatDb = postgres.AddDatabase(Components.Database.Chat);
var userDb = postgres.AddDatabase(Components.Database.User);
var ratingDb = postgres.AddDatabase(Components.Database.Rating);
var healthDb = postgres.AddDatabase(Components.Database.Health);
var catalogDb = postgres.AddDatabase(Components.Database.Catalog);
var financeDb = postgres.AddDatabase(Components.Database.Finance);
var orderingDb = postgres.AddDatabase(Components.Database.Ordering);
var notificationDb = postgres.AddDatabase(Components.Database.Notification);

await builder.AddOllama(configure =>
{
    configure.AddModel(Components.Ollama.Embedding, "nomic-embed-text:latest");
    configure.AddModel(
        Components.Ollama.Chat,
        $"gemma3:{(builder.ExecutionContext.IsPublishMode ? 4 : 1)}b"
    );
});

var keycloak = builder
    .AddKeycloak(Components.KeyCloak)
    .WithDataVolume()
    .WithCustomTheme(kcThemeName)
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithSampleRealmImport(kcRealmName, kcThemeDisplayName)
    .WithExternalDatabase(pgEndpoint, pgUser, pgPassword, userDb);

kcRealmName.WithParentRelationship(keycloak);
kcThemeName.WithParentRelationship(keycloak);
kcThemeDisplayName.WithParentRelationship(keycloak);

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
    .WithReference(chatDb)
    .WaitFor(chatDb)
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
    .WaitFor(queue)
    .WithIdP(keycloak, kcRealmName);

var gateway = builder
    .AddApiGatewayProxy()
    .WithService(catalogApi, true)
    .WithService(chatApi)
    .WithService(orderingApi)
    .WithService(basketApi, true)
    .WithService(ratingApi)
    .WithService(keycloak);

builder
    .AddHealthChecksUI()
    .WithStorageProvider(healthDb)
    .WithReference(catalogApi)
    .WithReference(chatApi)
    .WithReference(mcp)
    .WithReference(orderingApi)
    .WithReference(ratingApi)
    .WithReference(basketApi)
    .WithReference(notificationApi)
    .WithReference(financeApi)
    .WithExternalHttpEndpoints();

if (builder.ExecutionContext.IsRunMode)
{
    builder
        .AddScalar(keycloak)
        .WithOpenAPI(basketApi)
        .WithOpenAPI(catalogApi)
        .WithOpenAPI(chatApi)
        .WithOpenAPI(orderingApi)
        .WithOpenAPI(ratingApi)
        .WithOpenAPI(mcp);

    builder.AddK6(gateway);
}

builder.Build().Run();
