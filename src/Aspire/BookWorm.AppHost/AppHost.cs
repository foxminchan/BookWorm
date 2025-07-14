var builder = DistributedApplication.CreateBuilder(args);

builder.HidePlainHttpLink();

if (builder.ExecutionContext.IsPublishMode)
{
    builder.AddDashboard();
    builder.ConfigureCors();
    builder.AddAzureApplicationInsights(Components.Azure.ApplicationInsights);
    builder.AddAzureContainerAppEnvironment(Components.Azure.ContainerApp).ProvisionAsService();
}

var kcRealmName = builder.AddParameter("kc-realm", nameof(BookWorm).ToLowerInvariant());
var kcThemeName = builder.AddParameter("kc-theme", nameof(BookWorm).ToLowerInvariant());
var kcThemeDisplayName = builder.AddParameter("kc-theme-display-name", nameof(BookWorm));

var postgres = builder
    .AddAzurePostgresFlexibleServer(Components.Postgres)
    .RunAsContainer()
    .ProvisionAsService();

var redis = builder.AddAzureRedis(Components.Redis).RunAsContainer().ProvisionAsService();

var qdrant = builder
    .AddQdrant(Components.VectorDb)
    .WithDataVolume()
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithLifetime(ContainerLifetime.Persistent);

var queue = builder
    .AddRabbitMQ(Components.Queue)
    .WithManagementPlugin()
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint(Protocol.Tcp, e => e.Port = 5672);

var storage = builder
    .AddAzureStorage(Components.Azure.Storage.Resource)
    .RunAsContainer()
    .ProvisionAsService();

var signalR = builder
    .AddAzureSignalR(Components.Azure.SignalR)
    .RunAsContainer()
    .ProvisionAsService();

var blobStorage = storage
    .AddBlobs(Components.Azure.Storage.Blob)
    .AddBlobContainer(Components.Azure.Storage.BlobContainer);

var tableStorage = storage.AddTables(Components.Azure.Storage.Table);
var catalogDb = postgres.AddDatabase(Components.Database.Catalog);
var orderingDb = postgres.AddDatabase(Components.Database.Ordering);
var financeDb = postgres.AddDatabase(Components.Database.Finance);
var ratingDb = postgres.AddDatabase(Components.Database.Rating);
var chatDb = postgres.AddDatabase(Components.Database.Chat);
var healthDb = postgres.AddDatabase(Components.Database.Health);

builder.AddOllama(configure =>
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
    .RunWithHttpsDevCertificate();

var catalogApi = builder
    .AddProject<BookWorm_Catalog>(Application.Catalog)
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
    )
    .WithAzApplicationInsights()
    .WithOpenApi()
    .WithAsyncApi()
    .WithHealthCheck();

qdrant.WithParentRelationship(catalogApi);

var mcp = builder
    .AddProject<BookWorm_McpTools>(Application.McpTools)
    .WithReference(catalogApi)
    .WaitFor(catalogApi)
    .WithAzApplicationInsights();

var chatApi = builder
    .AddProject<BookWorm_Chat>(Application.Chatting)
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
    .WithRoleAssignments(signalR, SignalRBuiltInRole.SignalRContributor)
    .WithAzApplicationInsights()
    .WithOpenApi()
    .WithHealthCheck();

mcp.WithParentRelationship(chatApi);

var basketApi = builder
    .AddProject<BookWorm_Basket>(Application.Basket)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(catalogApi)
    .WithIdP(keycloak, kcRealmName)
    .WithAzApplicationInsights()
    .WithOpenApi()
    .WithAsyncApi()
    .WithHealthCheck();

var notificationApi = builder
    .AddProject<BookWorm_Notification>(Application.Notification)
    .WithEmailProvider()
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(tableStorage)
    .WaitFor(tableStorage)
    .WithRoleAssignments(storage, StorageBuiltInRole.StorageTableDataContributor)
    .WithAzApplicationInsights()
    .WithAsyncApi(true)
    .WithHealthCheck();

var orderingApi = builder
    .AddProject<BookWorm_Ordering>(Application.Ordering)
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
    .WithAzApplicationInsights()
    .WithOpenApi()
    .WithAsyncApi()
    .WithHmacSecret()
    .WithHealthCheck();

var ratingApi = builder
    .AddProject<BookWorm_Rating>(Application.Rating)
    .WithReference(ratingDb)
    .WaitFor(ratingDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithIdP(keycloak, kcRealmName)
    .WithReference(chatApi)
    .WithAzApplicationInsights()
    .WithOpenApi()
    .WithAsyncApi()
    .WithHealthCheck();

chatApi.WithReference(ratingApi).WaitFor(ratingApi);

var financeApi = builder
    .AddProject<BookWorm_Finance>(Application.Finance)
    .WithReference(financeDb)
    .WaitFor(financeDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithIdP(keycloak, kcRealmName)
    .WithAzApplicationInsights()
    .WithOpenApi()
    .WithAsyncApi()
    .WithHealthCheck();

var gateway = builder
    .AddYarp(Application.Gateway)
    .WithConfigFile("Container/proxy/yarp.json")
    .WithReference(catalogApi)
    .WaitFor(catalogApi)
    .WithReference(chatApi)
    .WaitFor(chatApi)
    .WithReference(orderingApi)
    .WaitFor(orderingApi)
    .WithReference(ratingApi)
    .WaitFor(ratingApi)
    .WithReference(basketApi)
    .WaitFor(basketApi)
    .WithReference(financeApi)
    .WaitFor(financeApi)
    .WithReference(keycloak);

var backOffice = builder
    .AddProject<BookWorm_BackOffice>(Application.BackOffice)
    .WithReference(catalogApi)
    .WaitFor(catalogApi)
    .WithReference(orderingApi)
    .WaitFor(orderingApi)
    .WithReference(basketApi)
    .WaitFor(basketApi)
    .WithReference(financeApi)
    .WaitFor(financeApi)
    .WithHealthCheck();

builder
    .AddHealthChecksUI()
    .WithExternalHttpEndpoints()
    .WithStorageProvider(healthDb)
    .WithReference(catalogApi)
    .WithReference(chatApi)
    .WithReference(mcp)
    .WithReference(orderingApi)
    .WithReference(ratingApi)
    .WithReference(basketApi)
    .WithReference(notificationApi)
    .WithReference(financeApi);

if (builder.ExecutionContext.IsRunMode)
{
    builder
        .AddScalar(keycloak)
        .WithApi(basketApi)
        .WithApi(catalogApi)
        .WithApi(chatApi)
        .WithApi(orderingApi)
        .WithApi(ratingApi)
        .WithApi(financeApi);

    builder.AddK6(gateway);

    builder.AddMcpInspector(Components.McpInspector).WithMcpServer(mcp).WaitFor(mcp);
}

builder.Build().Run();
