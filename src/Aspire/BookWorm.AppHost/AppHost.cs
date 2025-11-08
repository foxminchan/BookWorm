var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment(Components.Azure.ContainerApp).ProvisionAsService();

var schedulerUserName = builder
    .AddParameter("scheduler-user", "admin", true)
    .WithDescription(ParameterDescriptions.Scheduler.UserName, true);

var schedulerPassword = builder
    .AddParameter("scheduler-password", true)
    .WithDescription(ParameterDescriptions.Scheduler.Password, true)
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

var postgres = builder
    .AddAzurePostgresFlexibleServer(Components.Postgres)
    .WithPasswordAuthentication()
    .WithIconName("HomeDatabase")
    .RunAsLocalContainer()
    .ProvisionAsService();

var redis = builder
    .AddAzureRedis(Components.Redis)
    .WithIconName("Memory")
    .WithAccessKeyAuthentication()
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

var signalR = builder
    .AddAzureSignalR(Components.Azure.SignalR)
    .WithIconName("DesktopSignal")
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
    .AddProject<Catalog>(Services.Catalog)
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(catalogDb)
    .WaitFor(catalogDb)
    .WithReference(redis)
    .WaitFor(redis)
    .WithKeycloak(keycloak)
    .WithReference(blobStorage)
    .WaitFor(blobStorage)
    .WithReference(chat)
    .WithReference(embedding)
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
    .WithReference(chat)
    .WithReference(embedding)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(mcp)
    .WaitFor(mcp)
    .WithReference(qdrant)
    .WaitFor(qdrant)
    .WithKeycloak(keycloak)
    .WithReference(signalR)
    .WaitFor(signalR)
    .WithRoleAssignments(signalR, SignalRBuiltInRole.SignalRContributor)
    .WithUrlForEndpoint(
        Protocols.Http,
        url =>
        {
            url.DisplayText = "DevUI";
            url.Url = "/devui";
        }
    );

var basketApi = builder
    .AddProject<Basket>(Services.Basket)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(catalogApi)
    .WithKeycloak(keycloak);

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
    .WithKeycloak(keycloak)
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithReference(signalR)
    .WaitFor(signalR)
    .WithRoleAssignments(signalR, SignalRBuiltInRole.SignalRContributor)
    .WithHmacSecret();

var ratingApi = builder
    .AddProject<Rating>(Services.Rating)
    .WithReference(chat)
    .WithReference(embedding)
    .WithReference(ratingDb)
    .WaitFor(ratingDb)
    .WithReference(mcp)
    .WaitFor(mcp)
    .WithReference(queue)
    .WaitFor(queue)
    .WithKeycloak(keycloak)
    .WithReference(chatApi)
    .WithUrlForEndpoint(
        Protocols.Http,
        url =>
        {
            url.DisplayText = "DevUI";
            url.Url = "/devui";
        }
    );

chatApi.WithReference(ratingApi).WaitFor(ratingApi);

var financeApi = builder
    .AddProject<Finance>(Services.Finance)
    .WithReference(financeDb)
    .WaitFor(financeDb)
    .WithReference(queue)
    .WaitFor(queue);

var schedulerMigrator = builder
    .AddProject<SchedulerMigrator>(Services.SchedulerMigrator)
    .WithReference(schedulerDb)
    .WaitFor(schedulerDb);

var schedulerApi = builder
    .AddProject<Scheduler>(Services.Scheduler)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(schedulerDb)
    .WaitForCompletion(schedulerMigrator)
    .WithEnvironment("TickerQBasicAuth__Username", schedulerUserName)
    .WithEnvironment("TickerQBasicAuth__Password", schedulerPassword);

schedulerUserName.WithParentRelationship(schedulerApi);
schedulerMigrator.WithParentRelationship(schedulerApi);

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
        .AddHealthChecksUI()
        .WithReference(mcp)
        .WithReference(chatApi)
        .WithReference(ratingApi)
        .WithReference(basketApi)
        .WithReference(catalogApi)
        .WithReference(financeApi)
        .WithReference(orderingApi)
        .WithReference(schedulerApi)
        .WithReference(notificationApi);

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

builder.Build().Run();
