var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment(Components.Azure.ContainerApp).ProvisionAsService();

var postgres = builder
    .AddAzurePostgresFlexibleServer(Components.Postgres)
    .WithPasswordAuthentication()
    .WithIconName("HomeDatabase")
    .RunAsLocalContainer()
    .ProvisionAsService(builder.Environment);

var redis = builder
    .AddAzureManagedRedis(Components.Redis)
    .WithAccessKeyAuthentication()
    .WithIconName("Memory")
    .RunAsLocalContainer()
    .ProvisionAsService(builder.Environment);

var qdrant = builder
    .AddQdrant(Components.VectorDb)
    .WithIconName("DatabaseSearch")
    .WithDataVolume()
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithLifetime(ContainerLifetime.Persistent);

var queue = builder
    .AddKafka(Components.Broker)
    .WithIconName("Pipeline")
    .WithKafkaUI()
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithLifetime(ContainerLifetime.Persistent);

var storage = builder
    .AddAzureStorage(Components.Azure.Storage.Resource)
    .WithIconName("DatabasePlugConnected")
    .RunAsLocalContainer();

var catalogContainer = storage
    .AddBlobContainer(Components.Azure.Storage.BlobContainer(Services.Catalog))
    .WithAzureStorageExplorer();

var ratingDb = postgres.AddDatabase(Components.Database.Rating).WithPostgresMcp();
var catalogDb = postgres.AddDatabase(Components.Database.Catalog).WithPostgresMcp();
var financeDb = postgres.AddDatabase(Components.Database.Finance).WithPostgresMcp();
var orderingDb = postgres.AddDatabase(Components.Database.Ordering).WithPostgresMcp();
var notificationDb = postgres.AddDatabase(Components.Database.Notification).WithPostgresMcp();
var schedulerDb = postgres.AddDatabase(Components.Database.Scheduler).WithPostgresMcp();

var foundry = builder.AddFoundry(Components.Foundry.Resource);

var chat = foundry.AddDeployment(Components.Foundry.Chat, FoundryModel.Anthropic.ClaudeOpus5);

var embedding = foundry.AddDeployment(Components.Foundry.Embedding, FoundryModel.Cohere.EmbedV40);

IResourceBuilder<IResource> keycloak = builder.ExecutionContext.IsRunMode
    ? builder.AddLocalKeycloak(Components.KeyCloak)
    : builder.AddHostedKeycloak(Components.KeyCloak);

var presidioAnalyzer = builder
    .AddPresidioAnalyzer(Components.Presidio.Analyzer)
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithLifetime(ContainerLifetime.Persistent);

var presidioAnonymizer = builder
    .AddPresidioAnonymizer(Components.Presidio.Anonymizer)
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithLifetime(ContainerLifetime.Persistent);

var catalogApi = builder
    .AddDotnetProject(
        Services.Catalog,
        "../../Services/Catalog/BookWorm.Catalog/BookWorm.Catalog.csproj"
    )
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
    .WaitFor(chat)
    .WithReference(embedding)
    .WaitFor(embedding)
    .WithRoleAssignments(
        storage,
        StorageBuiltInRole.StorageBlobDataContributor,
        StorageBuiltInRole.StorageBlobDataOwner
    )
    .WithRoleAssignments(foundry, CognitiveServicesBuiltInRole.CognitiveServicesUser)
    .WithFriendlyUrls();

var mcp = builder
    .AddDotnetProject(
        Services.McpTools,
        "../../Services/McpTools/BookWorm.McpTools/BookWorm.McpTools.csproj"
    )
    .WithReference(catalogApi)
    .WithKeycloak(keycloak)
    .WithFriendlyUrls();

var basketApi = builder
    .AddDotnetProject(
        Services.Basket,
        "../../Services/Basket/BookWorm.Basket/BookWorm.Basket.csproj"
    )
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(catalogApi)
    .WithKeycloak(keycloak)
    .WithFriendlyUrls();

var orderingApi = builder
    .AddDotnetProject(
        Services.Ordering,
        "../../Services/Ordering/BookWorm.Ordering/BookWorm.Ordering.csproj"
    )
    .WithReference(orderingDb)
    .WaitFor(orderingDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(redis)
    .WaitFor(redis)
    .WithKeycloak(keycloak)
    .WithReference(catalogApi)
    .WaitFor(catalogApi)
    .WithReference(basketApi)
    .WithSecret("hmac-key", "HMAC__Key")
    .WithFriendlyUrls();

var chatApi = builder
    .AddDotnetProject(Services.Chatting, "../../Services/Chat/BookWorm.Chat/BookWorm.Chat.csproj")
    .WithReference(chat)
    .WaitFor(chat)
    .WithReference(embedding)
    .WaitFor(embedding)
    .WithReference(mcp)
    .WithKeycloak(keycloak)
    .WithReference(presidioAnalyzer)
    .WaitFor(presidioAnalyzer)
    .WithReference(presidioAnonymizer)
    .WaitFor(presidioAnonymizer)
    .WithRoleAssignments(foundry, CognitiveServicesBuiltInRole.CognitiveServicesUser)
    .WithFriendlyUrls();

var ratingApi = builder
    .AddDotnetProject(
        Services.Rating,
        "../../Services/Rating/BookWorm.Rating/BookWorm.Rating.csproj"
    )
    .WithReference(chat)
    .WaitFor(chat)
    .WithReference(embedding)
    .WaitFor(embedding)
    .WithReference(ratingDb)
    .WaitFor(ratingDb)
    .WithReference(mcp)
    .WithReference(queue)
    .WaitFor(queue)
    .WithKeycloak(keycloak)
    .WithReference(chatApi)
    .WaitFor(chatApi)
    .WithReference(presidioAnalyzer)
    .WaitFor(presidioAnalyzer)
    .WithReference(presidioAnonymizer)
    .WaitFor(presidioAnonymizer)
    .WithRoleAssignments(foundry, CognitiveServicesBuiltInRole.CognitiveServicesUser)
    .WithFriendlyUrls();

mcp.WithReference(ratingApi);

builder
    .AddDotnetProject(
        Services.Notification,
        "../../Services/Notification/BookWorm.Notification/BookWorm.Notification.csproj"
    )
    .WithEmailProvider()
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(notificationDb)
    .WaitFor(notificationDb)
    .WithFriendlyUrls(path: Http.Endpoints.AlivenessEndpointPath);

builder
    .AddDotnetProject(
        Services.Finance,
        "../../Services/Finance/BookWorm.Finance/BookWorm.Finance.csproj"
    )
    .WithReference(financeDb)
    .WaitFor(financeDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithFriendlyUrls(path: Http.Endpoints.AlivenessEndpointPath);

builder
    .AddDotnetProject(
        Services.Scheduler,
        "../../Services/Scheduler/BookWorm.Scheduler/BookWorm.Scheduler.csproj"
    )
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(schedulerDb)
    .WaitFor(schedulerDb)
    .WithFriendlyUrls("Quartz Dashboard", path: Http.Endpoints.QuartzDashboardEndpointPath)
    .WithExplicitStart();

var gateway = builder
    .AddApiGatewayProxy()
    .WithService(chatApi)
    .WithService(ratingApi)
    .WithService(orderingApi)
    .WithService(basketApi, true)
    .WithService(catalogApi, true)
    .Build();

var (storefront, backoffice) = builder.AddFrontendApps(gateway, keycloak);

if (builder.ExecutionContext.IsRunMode)
{
    builder
        .AddScalar(keycloak)
        .WithOpenAPI(chatApi)
        .WithOpenAPI(basketApi)
        .WithOpenAPI(ratingApi)
        .WithOpenAPI(catalogApi)
        .WithOpenAPI(orderingApi);

    builder.AddMcpInspector(Components.Inspector).WithMcpServer(mcp);

    builder
        .AddDevUI(Components.DevUI)
        .WithAgentService(
            chatApi,
            [
                new(Agents.QAAgent),
                new(Agents.RouterAgent),
                new(Agents.LanguageAgent),
                new(Agents.SummarizeAgent),
                new(Agents.SentimentAgent),
                new(Agents.BookAgent),
                new(Workflows.Chat),
            ]
        )
        .WithAgentService(ratingApi, [new(Agents.RatingAgent), new(Workflows.RatingSummarizer)])
        .WaitFor(chatApi)
        .WaitFor(ratingApi);
}
else
{
    storage.ProvisionAsService(builder.Environment);
    var frontendScheme = Uri.UriSchemeHttps;

    catalogApi.WithCorsOrigins(storefront, backoffice, frontendScheme);
    basketApi.WithCorsOrigins(storefront, backoffice, frontendScheme);
    orderingApi.WithCorsOrigins(storefront, backoffice, frontendScheme);
    chatApi.WithCorsOrigins(storefront, backoffice, frontendScheme);
    ratingApi.WithCorsOrigins(storefront, backoffice, frontendScheme);
}

await builder.Build().RunAsync();
