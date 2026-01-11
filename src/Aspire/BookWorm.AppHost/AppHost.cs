var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment(Components.Azure.ContainerApp).ProvisionAsService();

var registry = builder.AddContainerRegistry();

var postgres = builder
    .AddAzurePostgresFlexibleServer(Components.Postgres)
    .WithPasswordAuthentication()
    .WithIconName("HomeDatabase")
    .RunAsLocalContainer()
    .ProvisionAsService();

var redis = builder
    .AddAzureManagedRedis(Components.Redis)
    .WithAccessKeyAuthentication()
    .WithIconName("Memory")
    .RunAsLocalContainer()
    .ProvisionAsService();

var qdrant = builder
    .AddQdrant(Components.VectorDb)
    .WithIconName("DatabaseSearch")
    .WithDataVolume()
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithLifetime(ContainerLifetime.Persistent);

var queue = builder
    .AddRabbitMQ(Components.Queue)
    .WithIconName("Pipeline")
    .WithManagementPlugin()
    .WithDataVolume()
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint(Network.Tcp, e => e.Port = 5672);

var storage = builder
    .AddAzureStorage(Components.Azure.Storage.Resource)
    .WithIconName("DatabasePlugConnected")
    .RunAsLocalContainer()
    .ProvisionAsService();

var catalogContainer = storage.AddBlobContainer(
    Components.Azure.Storage.BlobContainer(Services.Catalog)
);

var ratingDb = postgres.AddDatabase(Components.Database.Rating);
var catalogDb = postgres.AddDatabase(Components.Database.Catalog);
var financeDb = postgres.AddDatabase(Components.Database.Finance);
var orderingDb = postgres.AddDatabase(Components.Database.Ordering);
var notificationDb = postgres.AddDatabase(Components.Database.Notification);

var openai = builder.AddOpenAI(Components.OpenAI.Resource);

var chat = openai
    .AddModel(Components.OpenAI.Chat, Components.OpenAI.OpenAIGpt4oMini)
    .WithHealthCheck();
var embedding = openai
    .AddModel(Components.OpenAI.Embedding, Components.OpenAI.TextEmbedding3Large)
    .WithHealthCheck();

IResourceBuilder<IResource> keycloak = builder.ExecutionContext.IsRunMode
    ? builder.AddLocalKeycloak(Components.KeyCloak)
    : builder.AddHostedKeycloak(Components.KeyCloak);

var catalogApi = builder
    .AddProject<BookWorm_Catalog>(Services.Catalog)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(catalogDb)
    .WaitFor(catalogDb)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(qdrant)
    .WaitFor(qdrant)
    .WithKeycloak(keycloak)
    .WithReference(catalogContainer)
    .WaitFor(catalogContainer)
    .WithReference(chat)
    .WithReference(embedding)
    .WithContainerRegistry(registry)
    .WithRoleAssignments(
        storage,
        StorageBuiltInRole.StorageBlobDataContributor,
        StorageBuiltInRole.StorageBlobDataOwner
    )
    .WithFriendlyUrls();

var mcp = builder
    .AddProject<BookWorm_McpTools>(Services.McpTools)
    .WithReference(catalogApi)
    .WithContainerRegistry(registry)
    .WithFriendlyUrls();

var basketApi = builder
    .AddProject<BookWorm_Basket>(Services.Basket)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(catalogApi)
    .WithKeycloak(keycloak)
    .WithContainerRegistry(registry)
    .WithFriendlyUrls();

var orderingApi = builder
    .AddProject<BookWorm_Ordering>(Services.Ordering)
    .WithReference(orderingDb)
    .WaitFor(orderingDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(redis)
    .WaitFor(redis)
    .WithKeycloak(keycloak)
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithSecret("hmac-key", "HMAC__Key")
    .WithContainerRegistry(registry)
    .WithFriendlyUrls();

var chatApi = builder
    .AddProject<BookWorm_Chat>(Services.Chatting)
    .WithReference(chat)
    .WithReference(embedding)
    .WithReference(mcp)
    .WithKeycloak(keycloak)
    .WithContainerRegistry(registry)
    .WithFriendlyUrls("Dev UI", path: Http.Endpoints.DevUIEndpointPath);

var ratingApi = builder
    .AddProject<BookWorm_Rating>(Services.Rating)
    .WithReference(chat)
    .WithReference(embedding)
    .WithReference(ratingDb)
    .WaitFor(ratingDb)
    .WithReference(mcp)
    .WithReference(queue)
    .WaitFor(queue)
    .WithKeycloak(keycloak)
    .WithReference(chatApi)
    .WaitFor(chatApi)
    .WithContainerRegistry(registry)
    .WithFriendlyUrls("Dev UI", path: Http.Endpoints.DevUIEndpointPath);

builder
    .AddProject<BookWorm_Notification>(Services.Notification)
    .WithEmailProvider()
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(notificationDb)
    .WaitFor(notificationDb)
    .WithContainerRegistry(registry)
    .WithFriendlyUrls(path: Http.Endpoints.AlivenessEndpointPath);

builder
    .AddProject<BookWorm_Finance>(Services.Finance)
    .WithReference(financeDb)
    .WaitFor(financeDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithContainerRegistry(registry)
    .WithFriendlyUrls(path: Http.Endpoints.AlivenessEndpointPath);

builder
    .AddProject<BookWorm_Scheduler>(Services.Scheduler)
    .WithReference(queue)
    .WaitFor(queue)
    .WithContainerRegistry(registry)
    .WithFriendlyUrls(path: Http.Endpoints.AlivenessEndpointPath)
    .WithExplicitStart();

var gateway = builder
    .AddApiGatewayProxy()
    .WithService(chatApi)
    .WithService(ratingApi)
    .WithService(orderingApi)
    .WithService(basketApi, true)
    .WithService(catalogApi, true)
    .Build();

var turbo = builder
    .AddTurborepoApp(
        Components.TurboRepo,
        Path.GetFullPath("../../Clients", builder.AppHostDirectory)
    )
    .WithPnpm(true)
    .WithPackageManagerLaunch();

var storefront = turbo
    .AddApp(Clients.StoreFront, Clients.StoreFrontTurboApp)
    .WithOtlpExporter()
    .WithHttpEndpoint(env: "PORT")
    .WithMappedEndpointPort()
    .WithHttpHealthCheck()
    .WithExternalHttpEndpoints()
    .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTPS", gateway.GetEndpoint(Http.Schemes.Https))
    .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTP", gateway.GetEndpoint(Http.Schemes.Http))
    .WithEnvironment("NEXT_PUBLIC_COPILOT_ENABLED", "true")
    .WithKeycloak(keycloak);

storefront.WithEnvironment("NEXT_PUBLIC_APP_URL", storefront.GetEndpoint(Http.Schemes.Http));

var backoffice = turbo
    .AddApp(Clients.BackOffice, Clients.BackOfficeTurboApp)
    .WithOtlpExporter()
    .WithHttpEndpoint(env: "PORT")
    .WithMappedEndpointPort()
    .WithHttpHealthCheck()
    .WithExternalHttpEndpoints()
    .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTPS", gateway.GetEndpoint(Http.Schemes.Https))
    .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTP", gateway.GetEndpoint(Http.Schemes.Http))
    .WithKeycloak(keycloak);

backoffice.WithEnvironment("NEXT_PUBLIC_APP_URL", backoffice.GetEndpoint(Http.Schemes.Http));

if (builder.ExecutionContext.IsRunMode)
{
    builder
        .AddScalar(keycloak)
        .WithOpenAPI(mcp)
        .WithOpenAPI(chatApi)
        .WithOpenAPI(basketApi)
        .WithOpenAPI(ratingApi)
        .WithOpenAPI(catalogApi)
        .WithOpenAPI(orderingApi);

    builder.AddMcpInspector(Components.Inspector).WithMcpServer(mcp);

    builder.AddK6(gateway);
}

await builder.Build().RunAsync();
