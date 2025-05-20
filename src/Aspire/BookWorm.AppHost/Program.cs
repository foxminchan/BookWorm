using Azure.Provisioning.SignalR;
using Azure.Provisioning.Storage;
using BookWorm.AppHost.Extensions;
using BookWorm.Constants.Aspire;
using BookWorm.HealthChecksUI;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("aca");

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
    .WithEndpoint("tcp", e => e.Port = 5672);

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

builder.AddOllama(configure: configure =>
{
    configure.AddModel(Components.Ollama.Embedding, "nomic-embed-text:latest");
    configure.AddModel(Components.Ollama.Chat, "deepseek-r1:1.5b");
});

var keycloak = builder
    .AddKeycloak(Components.KeyCloak)
    .WithDataVolume()
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithSampleRealmImport(nameof(BookWorm).ToLowerInvariant(), nameof(BookWorm));

var catalogApi = builder
    .AddProject<BookWorm_Catalog>(Application.Catalog)
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
    .WithOllama()
    .WithReference(blobStorage)
    .WaitFor(blobStorage)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(catalogDb)
    .WaitFor(catalogDb)
    .WithReference(qdrant)
    .WaitFor(qdrant)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(redis)
    .WaitFor(redis)
    .WithRoleAssignments(storage, StorageBuiltInRole.StorageBlobDataContributor)
    .WithUrls(c => c.Urls.ForEach(u => u.DisplayText = $"Open API ({u.Endpoint?.EndpointName})"));

qdrant.WithParentRelationship(catalogApi);

var mcp = builder
    .AddProject<BookWorm_McpTools>(Application.McpTools)
    .WithOllama()
    .WithReference(qdrant)
    .WaitFor(qdrant)
    .WithReference(catalogApi)
    .WaitFor(catalogApi);

var chatApi = builder
    .AddProject<BookWorm_Chat>(Application.Chatting)
    .WithOllama()
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(signalR)
    .WaitFor(signalR)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(mcp)
    .WaitFor(mcp)
    .WithRoleAssignments(signalR, SignalRBuiltInRole.SignalRContributor)
    .WithUrls(c => c.Urls.ForEach(u => u.DisplayText = $"Open API ({u.Endpoint?.EndpointName})"));

mcp.WithParentRelationship(chatApi);

var basketApi = builder
    .AddProject<BookWorm_Basket>(Application.Basket)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(catalogApi)
    .WithUrls(c => c.Urls.ForEach(u => u.DisplayText = $"Open API ({u.Endpoint?.EndpointName})"));

var notificationApi = builder
    .AddProject<BookWorm_Notification>(Application.Notification)
    .WithEmailProvider()
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(tableStorage)
    .WaitFor(tableStorage)
    .WithRoleAssignments(storage, StorageBuiltInRole.StorageTableDataContributor);

var orderingApi = builder
    .AddProject<BookWorm_Ordering>(Application.Ordering)
    .WithReference(orderingDb)
    .WaitFor(orderingDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithUrls(c => c.Urls.ForEach(u => u.DisplayText = $"Open API ({u.Endpoint?.EndpointName})"));

var ratingApi = builder
    .AddProject<BookWorm_Rating>(Application.Rating)
    .WithReference(ratingDb)
    .WaitFor(ratingDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithUrls(c => c.Urls.ForEach(u => u.DisplayText = $"Open API ({u.Endpoint?.EndpointName})"));

var financeApi = builder
    .AddProject<BookWorm_Finance>(Application.Finance)
    .WithReference(financeDb)
    .WaitFor(financeDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithUrls(c => c.Urls.ForEach(u => u.DisplayText = $"Open API ({u.Endpoint?.EndpointName})"));

var gateway = builder
    .AddYarp(Application.Gateway)
    .WithConfigFile("Container/proxy/yarp.json")
    .WithReference(catalogApi)
    .WithReference(chatApi)
    .WithReference(orderingApi)
    .WithReference(ratingApi)
    .WithReference(basketApi)
    .WithReference(financeApi)
    .WithReference(keycloak);

builder.AddK6(gateway);

builder
    .AddHealthChecksUi()
    .WithExternalHttpEndpoints()
    .WithReference(catalogApi)
    .WithReference(chatApi)
    .WithReference(mcp)
    .WithReference(orderingApi)
    .WithReference(ratingApi)
    .WithReference(basketApi)
    .WithReference(notificationApi)
    .WithReference(financeApi);

builder.Build().Run();
