using Azure.Provisioning.SignalR;
using Azure.Provisioning.Storage;
using BookWorm.AppHost.Extensions;
using BookWorm.Constants;
using BookWorm.HealthChecksUI;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProjectPublisher();

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

var storage = builder.AddAzureStorage(Components.Storage).RunAsContainer().ProvisionAsService();
var signalR = builder.AddAzureSignalR(Components.SignalR).RunAsContainer().ProvisionAsService();

var blobStorage = storage.AddBlobs(Components.Blob);
var catalogDb = postgres.AddDatabase(Components.Database.Catalog);
var orderingDb = postgres.AddDatabase(Components.Database.Ordering);
var financeDb = postgres.AddDatabase(Components.Database.Finance);
var ratingDb = postgres.AddDatabase(Components.Database.Rating);

var models = new Dictionary<string, string>
{
    { Components.Ollama.Embedding, "nomic-embed-text:latest" },
    { Components.Ollama.Chat, "deepseek-r1:1.5b" },
};

var keycloak = builder
    .AddKeycloak(Components.KeyCloak)
    .WithDataVolume()
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithLifetime(ContainerLifetime.Persistent);

if (builder.ExecutionContext.IsRunMode)
{
    keycloak.WithRealmImport("./realm-export.json");
}

var catalogApi = builder
    .AddProject<BookWorm_Catalog>(Application.Catalog)
    .WithReplicas(2)
    .WithScalarApiClient()
    .RunAsOllama(models)
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
    .WithReference(signalR)
    .WaitFor(signalR)
    .WithRoleAssignments(storage, StorageBuiltInRole.StorageBlobDataContributor)
    .WithRoleAssignments(signalR, SignalRBuiltInRole.SignalRContributor);

qdrant.WithParentRelationship(catalogApi);

var basketApi = builder
    .AddProject<BookWorm_Basket>(Application.Basket)
    .WithScalarApiClient()
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(catalogApi);

var notificationApi = builder
    .AddProject<BookWorm_Notification>(Application.Notification)
    .WithEmailProvider()
    .WithReference(queue)
    .WaitFor(queue);

var orderingApi = builder
    .AddProject<BookWorm_Ordering>(Application.Ordering)
    .WithScalarApiClient()
    .WithReference(orderingDb)
    .WaitFor(orderingDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(catalogApi)
    .WithReference(basketApi);

var ratingApi = builder
    .AddProject<BookWorm_Rating>(Application.Rating)
    .WithScalarApiClient()
    .WithReference(ratingDb)
    .WaitFor(ratingDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(keycloak)
    .WaitFor(keycloak);

var financeApi = builder
    .AddProject<BookWorm_Finance>(Application.Finance)
    .WithScalarApiClient()
    .WithReference(financeDb)
    .WaitFor(financeDb)
    .WithReference(queue)
    .WaitFor(queue);

var gateway = builder
    .AddProject<BookWorm_Gateway>(Application.Gateway)
    .WithExternalHttpEndpoints()
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(ratingApi)
    .WithReference(basketApi)
    .WithReference(financeApi)
    .WithReference(keycloak);

builder
    .AddHealthChecksUi()
    .WithExternalHttpEndpoints()
    .WithReference(gateway)
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(ratingApi)
    .WithReference(basketApi)
    .WithReference(notificationApi)
    .WithReference(financeApi);

builder.Build().Run();
