var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment(Components.Azure.ContainerApp).ProvisionAsService();

// Add Azure Managed Redis, Qdrant, and Kafka
var redis = builder
    .AddAzureManagedRedis(Components.Redis)
    .WithAccessKeyAuthentication()
    .RunAsLocalContainer()
    .ProvisionAsService(builder.Environment);

var qdrant = builder.ExecutionContext.IsRunMode
    ? builder
        .AddQdrant(Components.VectorDb)
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent)
    : builder.AddConnectionString(Components.VectorDb);

var broker = builder.ExecutionContext.IsRunMode
    ? builder.AddKafka(Components.Broker).WithKafkaUI().WithLifetime(ContainerLifetime.Persistent)
    : builder.AddConnectionString(Components.Broker);

// Add Azure Blob Storage
var storage = builder.AddAzureStorage(Components.Azure.Storage.Resource).RunAsLocalContainer();

var catalogContainer = storage
    .AddBlobContainer(Components.Azure.Storage.BlobContainer(Services.Catalog))
    .WithAzureStorageExplorer();

var chatSessionContainer = storage
    .AddBlobContainer(Components.Azure.Storage.BlobContainer(Services.Chatting))
    .WithAzureStorageExplorer();

// Add Azure Postgres Flexible Server and Databases
var postgres = builder
    .AddAzurePostgresFlexibleServer(Components.Postgres)
    .WithPasswordAuthentication()
    .RunAsLocalContainer()
    .ProvisionAsService(builder.Environment);

var catalogDb = postgres.AddDatabase(Components.Database.Catalog).WithPostgresMcp();
var financeDb = postgres.AddDatabase(Components.Database.Finance).WithPostgresMcp();
var orderingDb = postgres.AddDatabase(Components.Database.Ordering).WithPostgresMcp();
var notificationDb = postgres.AddDatabase(Components.Database.Notification).WithPostgresMcp();
var schedulerDb = postgres.AddDatabase(Components.Database.Scheduler).WithPostgresMcp();

// Add Azure Cosmos DB
var cosmos = builder
    .AddAzureCosmosDB(Components.Cosmos)
    .RunAsLocalContainer()
    .ProvisionAsService(builder.Environment);

var ratingDb = cosmos.AddCosmosDatabase(Components.Database.Rating);
var feedbacks = ratingDb.AddContainer("feedbacks", "/bookId");

// Add AI and Foundry components
var foundry = builder.AddFoundry(Components.Foundry.Resource);
var project = foundry.AddProject(Components.Foundry.Project);

project.AddModelDeployment(Components.Foundry.Chat, FoundryModel.Microsoft.ModelRouter);
project.AddModelDeployment(Components.Foundry.Embedding, FoundryModel.Cohere.EmbedV40);

var presidioAnalyzer = builder
    .AddPresidioAnalyzer(Components.Presidio.Analyzer)
    .WithLifetime(ContainerLifetime.Persistent);

var presidioAnonymizer = builder
    .AddPresidioAnonymizer(Components.Presidio.Anonymizer)
    .WithLifetime(ContainerLifetime.Persistent);

// Add Azure Entra ID and Agent Identities
var entraId = builder.AddEntraId(Components.Azure.EntraId);
var entraAgentIdentities = builder.AddEntraAgentIdentities("entra-agent-identities", entraId);

// Add services and their dependencies
var catalogApi = builder
    .AddDotnetProject(
        Services.Catalog,
        "../../Services/Catalog/BookWorm.Catalog/BookWorm.Catalog.csproj"
    )
    .WithReference(broker)
    .WaitFor(broker)
    .WithReference(catalogDb)
    .WaitFor(catalogDb)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(qdrant)
    .WaitFor(qdrant)
    .WithEntraId(entraId)
    .WithReference(catalogContainer)
    .WaitFor(catalogContainer)
    .WithReference(project)
    .WithRoleAssignments(
        storage,
        StorageBuiltInRole.StorageBlobDataContributor,
        StorageBuiltInRole.StorageBlobDataOwner
    )
    .WithRoleAssignments(foundry, CognitiveServicesBuiltInRole.CognitiveServicesUser)
    .WithDisplayUrls();

var mcp = builder
    .AddDotnetProject(
        Services.McpTools,
        "../../Services/McpTools/BookWorm.McpTools/BookWorm.McpTools.csproj"
    )
    .WithReference(catalogApi)
    .WithEntraId(entraId)
    .WithDisplayUrls();

var basketApi = builder
    .AddDotnetProject(
        Services.Basket,
        "../../Services/Basket/BookWorm.Basket/BookWorm.Basket.csproj"
    )
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(broker)
    .WaitFor(broker)
    .WithReference(catalogApi)
    .WithEntraId(entraId)
    .WithDisplayUrls();

var orderingApi = builder
    .AddDotnetProject(
        Services.Ordering,
        "../../Services/Ordering/BookWorm.Ordering/BookWorm.Ordering.csproj"
    )
    .WithReference(orderingDb)
    .WaitFor(orderingDb)
    .WithReference(broker)
    .WaitFor(broker)
    .WithReference(redis)
    .WaitFor(redis)
    .WithEntraId(entraId)
    .WithReference(catalogApi)
    .WaitFor(catalogApi)
    .WithReference(basketApi)
    .WithSecret("hmac-key", "HMAC__Key")
    .WithDisplayUrls();

var chatApi = builder
    .AddDotnetProject(Services.Chatting, "../../Services/Chat/BookWorm.Chat/BookWorm.Chat.csproj")
    .WithReference(project)
    .WithReference(mcp)
    .WaitFor(mcp)
    .WithEntraId(entraId)
    .WithEntraAgentIdentity(entraAgentIdentities, "chatAgentId", (Services.Rating, "ratingAgentId"))
    .WithReference(presidioAnalyzer)
    .WaitFor(presidioAnalyzer)
    .WithReference(presidioAnonymizer)
    .WaitFor(presidioAnonymizer)
    .WithReference(chatSessionContainer)
    .WaitFor(chatSessionContainer)
    .WithRoleAssignments(storage, StorageBuiltInRole.StorageBlobDataContributor)
    .WithRoleAssignments(foundry, CognitiveServicesBuiltInRole.CognitiveServicesUser)
    .WithDisplayUrls();

var ratingApi = builder
    .AddDotnetProject(
        Services.Rating,
        "../../Services/Rating/BookWorm.Rating/BookWorm.Rating.csproj"
    )
    .WithReference(project)
    .WithReference(feedbacks)
    .WaitFor(feedbacks)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(mcp)
    .WithReference(broker)
    .WaitFor(broker)
    .WithEntraId(entraId)
    .WithEntraAgentIdentity(entraAgentIdentities, "ratingAgentId")
    .WithReference(presidioAnalyzer)
    .WithReference(presidioAnonymizer)
    .WaitFor(presidioAnonymizer)
    .WithRoleAssignments(foundry, CognitiveServicesBuiltInRole.CognitiveServicesUser)
    .WithDisplayUrls();

chatApi.WithReference(ratingApi).WaitFor(ratingApi);

builder
    .AddDotnetProject(
        Services.Notification,
        "../../Services/Notification/BookWorm.Notification/BookWorm.Notification.csproj"
    )
    .WithEmailProvider()
    .WithReference(broker)
    .WaitFor(broker)
    .WithReference(notificationDb)
    .WaitFor(notificationDb)
    .WithDisplayUrls(path: Http.Endpoints.AlivenessEndpointPath);

builder
    .AddDotnetProject(
        Services.Finance,
        "../../Services/Finance/BookWorm.Finance/BookWorm.Finance.csproj"
    )
    .WithReference(financeDb)
    .WaitFor(financeDb)
    .WithReference(broker)
    .WaitFor(broker)
    .WithDisplayUrls(path: Http.Endpoints.AlivenessEndpointPath);

var catalogIngestion = builder
    .AddAzureFunctionsProject(
        Services.CatalogIngestion,
        "../../Services/Catalog/BookWorm.Catalog.Ingestion/BookWorm.Catalog.Ingestion.csproj"
    )
    .WithEntraId(entraId)
    .WithHostStorage(storage)
    .WithReference(catalogDb)
    .WaitFor(catalogApi)
    .WithReference(qdrant)
    .WaitFor(qdrant)
    .WithReference(project)
    .WithRoleAssignments(foundry, CognitiveServicesBuiltInRole.CognitiveServicesUser)
    .WithRoleAssignments(
        storage,
        StorageBuiltInRole.StorageBlobDataReader,
        StorageBuiltInRole.StorageQueueDataReader
    );

builder
    .AddDotnetProject(
        Services.Scheduler,
        "../../Services/Scheduler/BookWorm.Scheduler/BookWorm.Scheduler.csproj"
    )
    .WithEntraId(entraId)
    .WithReference(broker)
    .WaitFor(broker)
    .WithReference(schedulerDb)
    .WaitFor(schedulerDb)
    .WithReference(catalogIngestion)
    .WaitFor(catalogIngestion)
    .WithDisplayUrls("Quartz Dashboard", path: Http.Endpoints.QuartzDashboardEndpointPath)
    .WithExplicitStart();

var gateway = builder
    .AddApiGatewayProxy()
    .WithService(chatApi)
    .WithService(ratingApi)
    .WithService(orderingApi)
    .WithService(basketApi, true)
    .WithService(catalogApi, true)
    .Build();

var (storefront, backoffice) = builder.AddFrontendApps(gateway, entraId);

// Add CORS origins for the APIs based on the environment
if (builder.ExecutionContext.IsRunMode)
{
    builder.AddScalar(entraId).WithOpenAPI(chatApi, basketApi, ratingApi, catalogApi, orderingApi);

    builder.AddMcpInspector(Components.Inspector).WithMcpServer(mcp);
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
