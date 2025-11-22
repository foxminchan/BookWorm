var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment(Components.Azure.ContainerApp).ProvisionAsService();

var postgres = builder
    .AddAzurePostgresFlexibleServer(Components.Postgres)
    .WithPasswordAuthentication()
    .WithIconName("HomeDatabase")
    .RunAsLocalContainer()
    .ProvisionAsService();

var redis = builder
    .AddAzureRedis(Components.Redis)
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
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint(Network.Tcp, e => e.Port = 5672);

var storage = builder
    .AddAzureStorage(Components.Azure.Storage.Resource)
    .WithIconName("DatabasePlugConnected")
    .RunAsLocalContainer()
    .ProvisionAsService();

var blobStorage = storage.AddBlobContainer(Components.Azure.Storage.BlobContainer);

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
    .WithReference(blobStorage)
    .WaitFor(blobStorage)
    .WithReference(chat)
    .WithReference(embedding)
    .WithRoleAssignments(
        storage,
        StorageBuiltInRole.StorageBlobDataContributor,
        StorageBuiltInRole.StorageBlobDataOwner
    )
    .WithFriendlyUrls("Catalog (OpenAPI)");

var mcp = builder
    .AddProject<BookWorm_McpTools>(Services.McpTools)
    .WithReference(catalogApi)
    .WithFriendlyUrls("MCP (OpenAPI)");

var basketApi = builder
    .AddProject<BookWorm_Basket>(Services.Basket)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(catalogApi)
    .WithKeycloak(keycloak)
    .WithFriendlyUrls("Basket (OpenAPI)");

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
    .WithFriendlyUrls("Ordering (OpenAPI)");

var chatApi = builder
    .AddProject<BookWorm_Chat>(Services.Chatting)
    .WithReference(chat)
    .WithReference(embedding)
    .WithReference(mcp)
    .WithKeycloak(keycloak)
    .WithFriendlyUrls("Dev UI", path: "/devui");

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
    .WithFriendlyUrls("Dev UI", path: "/devui");

builder
    .AddProject<BookWorm_Notification>(Services.Notification)
    .WithEmailProvider()
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(notificationDb)
    .WaitFor(notificationDb)
    .WithFriendlyUrls("Notification (Status)", path: "/alive");

builder
    .AddProject<BookWorm_Finance>(Services.Finance)
    .WithReference(financeDb)
    .WaitFor(financeDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithFriendlyUrls("Finance (Status)", path: "/alive");

builder
    .AddProject<BookWorm_Scheduler>(Services.Scheduler)
    .WithReference(queue)
    .WaitFor(queue)
    .WithFriendlyUrls("Scheduler (Status)", path: "/alive")
    .WithExplicitStart();

var gateway = builder
    .AddApiGatewayProxy()
    .WithService(chatApi)
    .WithService(ratingApi)
    .WithService(orderingApi)
    .WithService(basketApi, true)
    .WithService(catalogApi, true)
    .WithService(keycloak);

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
