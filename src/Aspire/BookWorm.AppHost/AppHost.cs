using BookWorm.AppHost.Extensions.Proxy;

var builder = DistributedApplication.CreateBuilder(args);

var containerAppEnvironment = builder
    .AddAzureContainerAppEnvironment(Components.Azure.ContainerApp)
    .ProvisionAsService(builder.Environment);

// Add Azure Managed Redis, Qdrant, and Kafka
var redis = builder
    .AddAzureManagedRedis(Components.Redis)
    .RunAsLocalContainer()
    .ProvisionAsService(builder.Environment)
    .InGroup(ResourceGroups.Data);

var qdrant = builder.ExecutionContext.IsRunMode
    ? builder
        .AddQdrant(Components.VectorDb)
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent)
    : builder.AddConnectionString(Components.VectorDb);

qdrant.InGroup(ResourceGroups.Data);

var broker = builder.ExecutionContext.IsRunMode
    ? builder.AddKafka(Components.Broker).WithKafkaUI().WithLifetime(ContainerLifetime.Persistent)
    : builder.AddConnectionString(Components.Broker);

broker.InGroup(ResourceGroups.Messaging);

// Add Azure Blob Storage
var storage = builder
    .AddAzureStorage(Components.Azure.Storage.Resource)
    .RunAsLocalContainer()
    .InGroup(ResourceGroups.Data);

var catalogContainer = storage
    .AddBlobContainer(Components.Azure.Storage.BlobContainer(Services.Catalog))
    .WithAzureStorageExplorer();

var chatSessionContainer = storage
    .AddBlobContainer(Components.Azure.Storage.BlobContainer(Services.Chatting))
    .WithAzureStorageExplorer();

// Add Azure Postgres Flexible Server and Databases
var postgres = builder
    .AddAzurePostgresFlexibleServer(Components.Postgres)
    .RunAsLocalContainer()
    .ProvisionAsService(builder.Environment)
    .InGroup(ResourceGroups.Data);

var catalogDb = postgres.AddDatabase(Components.Database.Catalog).WithPostgresMcp();
var orderingDb = postgres.AddDatabase(Components.Database.Ordering).WithPostgresMcp();
var schedulerDb = postgres.AddDatabase(Components.Database.Scheduler).WithPostgresMcp();

// Add Azure Cosmos DB
var cosmos = builder
    .AddAzureCosmosDB(Components.Cosmos)
    .RunAsLocalContainer()
    .ProvisionAsService(builder.Environment)
    .InGroup(ResourceGroups.Data);

var ratingDb = cosmos.AddCosmosDatabase(Components.Database.Rating);
var feedbacks = ratingDb.AddContainer(Components.Container.Feedbacks, "/bookId");

// Add AI and Foundry components
var foundry = builder.AddFoundry(Components.Foundry.Resource).InGroup(ResourceGroups.AI);
var project = foundry.AddProject(Components.Foundry.Project);

project.AddModelDeployment(Components.Foundry.Chat, FoundryModel.Microsoft.ModelRouter);
project.AddModelDeployment(Components.Foundry.Decision, FoundryModel.Microsoft.MicrosoftDecision1);
project.AddModelDeployment(Components.Foundry.Embedding, FoundryModel.Cohere.EmbedV40);

var presidioAnalyzer = builder
    .AddPresidioAnalyzer(Components.Presidio.Analyzer)
    .WithLifetime(ContainerLifetime.Persistent)
    .InGroup(ResourceGroups.AI);

var presidioAnonymizer = builder
    .AddPresidioAnonymizer(Components.Presidio.Anonymizer)
    .WithLifetime(ContainerLifetime.Persistent)
    .InGroup(ResourceGroups.AI);

// Add Azure Entra ID and Agent Identities
var entraId = builder.AddEntraId(Components.Azure.EntraId).InGroup(ResourceGroups.Security);
var entraAgentIdentities = builder.AddEntraAgentIdentities(
    Components.Azure.EntraAgentIdentities,
    entraId
);

// Add services and their dependencies
var catalogApi = builder
    .AddDotnetProject(Services.Catalog, RepositoryPaths.ServiceProjectPath("Catalog"))
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
    .WithRoleAssignments(storage, StorageBuiltInRole.StorageBlobDataContributor)
    .WithRoleAssignments(foundry, CognitiveServicesBuiltInRole.CognitiveServicesUser)
    .WithDisplayUrls()
    .InGroup(ResourceGroups.Services, ResourceDisplayOrder.Services);

var mcp = builder
    .AddDotnetProject(Services.McpTools, RepositoryPaths.ServiceProjectPath("McpTools"))
    .WithReference(catalogApi)
    .WithEntraId(entraId)
    .WithDisplayUrls()
    .InGroup(ResourceGroups.Services, ResourceDisplayOrder.Services);

var basketApi = builder
    .AddDotnetProject(Services.Basket, RepositoryPaths.ServiceProjectPath("Basket"))
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(broker)
    .WaitFor(broker)
    .WithReference(catalogApi)
    .WithEntraId(entraId)
    .WithSecret("hmac-key", "HMAC__Key")
    .WithDisplayUrls()
    .InGroup(ResourceGroups.Services, ResourceDisplayOrder.Services);

var orderingApi = builder
    .AddDotnetProject(Services.Ordering, RepositoryPaths.ServiceProjectPath("Ordering"))
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
    .WithDisplayUrls()
    .InGroup(ResourceGroups.Services, ResourceDisplayOrder.Services);

var chatApi = builder
    .AddDotnetProject(Services.Chatting, RepositoryPaths.ServiceProjectPath("Chat"))
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
    .WithDisplayUrls()
    .InGroup(ResourceGroups.Services, ResourceDisplayOrder.Services);

var ratingApi = builder
    .AddDotnetProject(Services.Rating, RepositoryPaths.ServiceProjectPath("Rating"))
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
    .WithDisplayUrls()
    .InGroup(ResourceGroups.Services, ResourceDisplayOrder.Services);

chatApi.WithReference(ratingApi).WaitFor(ratingApi);

builder
    .AddDotnetProject(Services.Notification, RepositoryPaths.ServiceProjectPath("Notification"))
    .WithEmailProvider()
    .WithReference(broker)
    .WaitFor(broker)
    .WithReference(redis)
    .WaitFor(redis)
    .WithSecret("hmac-key", "HMAC__Key")
    .WithDisplayUrls(path: Http.Endpoints.AlivenessEndpointPath)
    .InGroup(ResourceGroups.Services, ResourceDisplayOrder.Services);

var quartzDashboard = builder
    .AddQuartzDashboard(Components.QuartzDashboard)
    .WithExternalHttpEndpoints()
    .WithEntraId(entraId)
    .InGroup(ResourceGroups.Tooling, ResourceDisplayOrder.Tooling);

builder
    .AddDotnetProject(Services.Scheduler, RepositoryPaths.ServiceProjectPath("Scheduler"))
    .WithReference(broker)
    .WaitFor(broker)
    .WithReference(schedulerDb)
    .WaitFor(schedulerDb)
    .WithReference(quartzDashboard)
    .WithDisplayUrls(path: Http.Endpoints.AlivenessEndpointPath)
    .WithExplicitStart()
    .InGroup(ResourceGroups.Services, ResourceDisplayOrder.Services);

// Add Functions projects for Finance and Catalog Ingestion
builder
    .AddAzureFunctionsProject(Services.Finance, RepositoryPaths.ServiceProjectPath("Finance"))
    .WithHostStorage(storage)
    .WithDurableTaskHub(
        Components.Azure.DurableTask.Scheduler,
        Components.Azure.DurableTask.TaskHub
    )
    .WithReference(broker)
    .WaitFor(broker)
    .WithEnvironment("BrokerList", broker.Resource.ConnectionStringExpression)
    .WithRoleAssignments(storage, StorageBuiltInRole.StorageBlobDataOwner)
    .InGroup(ResourceGroups.Functions, ResourceDisplayOrder.Functions);

builder
    .AddAzureFunctionsProject(
        Services.CatalogIngestion,
        RepositoryPaths.ServiceProjectPath("Catalog", "BookWorm.Catalog.Ingestion")
    )
    .WithHostStorage(storage)
    .WithReference(broker)
    .WaitFor(broker)
    .WithEnvironment("BrokerList", broker.Resource.ConnectionStringExpression)
    .WithReference(catalogDb)
    .WaitFor(catalogDb)
    .WithReference(qdrant)
    .WaitFor(qdrant)
    .WithReference(project)
    .WithRoleAssignments(foundry, CognitiveServicesBuiltInRole.CognitiveServicesUser)
    .WithRoleAssignments(storage, StorageBuiltInRole.StorageBlobDataOwner)
    .InGroup(ResourceGroups.Functions, ResourceDisplayOrder.Functions);

// Add API Gateway and Frontend Apps
var gatewayBuilder = builder
    .AddApiGatewayProxy()
    .WithService(chatApi)
    .WithService(ratingApi)
    .WithService(orderingApi)
    .WithService(basketApi)
    .WithService(catalogApi)
    .WithEntraProxy(entraId);

gatewayBuilder.Resource.WithComputeEnvironment(containerAppEnvironment);

var (storefront, commerce, backoffice) = builder.AddFrontendApps(gatewayBuilder.Resource, entraId);

gatewayBuilder
    .WithFrontend(backoffice, "/backoffice", zoneName: Clients.BackOffice)
    .WithMicrofrontends(storefront, commerce)
    .Build()
    .InGroup(ResourceGroups.Edge, ResourceDisplayOrder.Edge);

storefront.InGroup(ResourceGroups.Edge, ResourceDisplayOrder.Edge);
commerce.InGroup(ResourceGroups.Edge, ResourceDisplayOrder.Edge);
backoffice.InGroup(ResourceGroups.Edge, ResourceDisplayOrder.Edge);

// MCP tools are not routed through the gateway but still need a CORS policy outside Development
mcp.WithMcpCors(gatewayBuilder.Resource);

// Add Scalar and MCP Inspector in run mode, or provision storage in publish mode
if (builder.ExecutionContext.IsRunMode)
{
    builder
        .AddScalar(entraId)
        .InGroup(ResourceGroups.Tooling, ResourceDisplayOrder.Tooling)
        .WithOpenAPI(chatApi, basketApi, ratingApi, catalogApi, orderingApi);

    builder
        .AddMcpInspector(Components.Inspector)
        .WithMcpServer(mcp)
        .InGroup(ResourceGroups.Tooling, ResourceDisplayOrder.Tooling);
}
else
{
    storage.ProvisionAsService(builder.Environment);
    containerAppEnvironment.AssignComputeResources();
}

await builder.Build().RunAsync();
