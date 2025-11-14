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
    .WithEndpoint(Protocols.Tcp, e => e.Port = 5672);

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
var schedulerDb = postgres.AddDatabase(Components.Database.Scheduler);
var notificationDb = postgres.AddDatabase(Components.Database.Notification);

var openai = builder.AddOpenAI(Components.OpenAI.Resource);

var chat = openai.AddModel(Components.OpenAI.Chat, Components.OpenAI.OpenAIGpt4oMini);
var embedding = openai.AddModel(Components.OpenAI.Embedding, Components.OpenAI.TextEmbedding3Large);

IResourceBuilder<IResource> keycloak = builder.ExecutionContext.IsRunMode
    ? builder.AddLocalKeycloak(Components.KeyCloak).WithPostgres(postgres)
    : builder.AddHostedKeycloak(Components.KeyCloak);

var catalogApi = builder
    .AddProject<BookWorm_Catalog>(Services.Catalog)
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
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
    .WithHttpHealthCheck(Restful.Host.HealthEndpointPath)
    .WithRoleAssignments(
        storage,
        StorageBuiltInRole.StorageBlobDataContributor,
        StorageBuiltInRole.StorageBlobDataOwner
    );

var mcp = builder
    .AddProject<BookWorm_McpTools>(Services.McpTools)
    .WithReference(catalogApi)
    .WithHttpHealthCheck(Restful.Host.HealthEndpointPath);

var basketApi = builder
    .AddProject<BookWorm_Basket>(Services.Basket)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(catalogApi)
    .WithKeycloak(keycloak)
    .WithHttpHealthCheck(Restful.Host.HealthEndpointPath);

builder
    .AddProject<BookWorm_Notification>(Services.Notification)
    .WithEmailProvider()
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(notificationDb)
    .WaitFor(notificationDb)
    .WithHttpHealthCheck(Restful.Host.HealthEndpointPath);

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
    .WithHttpHealthCheck(Restful.Host.HealthEndpointPath);

var chatApi = builder
    .AddProject<BookWorm_Chat>(Services.Chatting)
    .WithReference(chat)
    .WithReference(embedding)
    .WithReference(mcp)
    .WithKeycloak(keycloak)
    .WithHttpHealthCheck(Restful.Host.HealthEndpointPath)
    .WithUrls(e =>
        e.Urls.Add(
            new()
            {
                Url = "/devui",
                DisplayText = "DevUI",
                Endpoint = e.GetEndpoint(Protocols.Http),
            }
        )
    );

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
    .WithHttpHealthCheck(Restful.Host.HealthEndpointPath)
    .WithUrls(e =>
        e.Urls.Add(
            new()
            {
                Url = "/devui",
                DisplayText = "DevUI",
                Endpoint = e.GetEndpoint(Protocols.Http),
            }
        )
    );

builder
    .AddProject<BookWorm_Finance>(Services.Finance)
    .WithReference(financeDb)
    .WaitFor(financeDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithHttpHealthCheck(Restful.Host.HealthEndpointPath);

builder
    .AddProject<BookWorm_Scheduler>(Services.Scheduler)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(schedulerDb)
    .WithSecret("api-key", "TickerQ__ApiKey")
    .WithHttpHealthCheck(Restful.Host.HealthEndpointPath)
    .WithUrls(c => c.Urls.ForEach(u => u.DisplayText = $"Dashboard ({u.Endpoint?.EndpointName})"));

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
