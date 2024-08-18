var builder = DistributedApplication.CreateBuilder(args);

builder.AddForwardedHeaders();

var postgresUser = builder.AddParameter("SqlUser", true);
var postgresPassword = builder.AddParameter("SqlPassword", true);
var fromEmail = builder.AddParameter("FromEmail", true);

var launchProfileName = builder.Configuration["DOTNET_LAUNCH_PROFILE"] ?? "http";

var postgres = builder
    .AddPostgres("postgres", postgresUser, postgresPassword, 5432)
    .WithPgAdmin()
    .WithImage("ankane/pgvector")
    .WithImageTag("latest")
    .WithDataBindMount("../../mnt/postgres");

var mongodb = builder
    .AddMongoDB("mongodb", 27017)
    .WithMongoExpress()
    .WithDataBindMount("../../mnt/mongodb");

var redis = builder.AddRedis("redis", 6379)
    .WithRedisCommander()
    .WithDataBindMount("../../mnt/redis");

var catalogDb = postgres.AddDatabase("catalogdb");
var orderingDb = postgres.AddDatabase("orderingdb");
var identityDb = postgres.AddDatabase("identitydb");
var notificationDb = postgres.AddDatabase("notificationdb");
var ratingDb = mongodb.AddDatabase("ratingdb");

var storage = builder.AddAzureStorage("storage");

if (builder.Environment.IsDevelopment())
{
    storage.RunAsEmulator(
        config => config.WithDataBindMount("../../mnt/azurite"));
}

var blobs = storage.AddBlobs("blobs");

var openAi = builder.AddConnectionString("openai");

var rabbitMq = builder
    .AddRabbitMQ("eventbus")
    .WithManagementPlugin();

var smtpServer = builder.AddMailDev("mailserver", 1080);

// Services
var identityApi = builder.AddProject<BookWorm_Identity>("identity-api")
    .WithReference(identityDb)
    .WithReference(redis)
    .WithExternalHttpEndpoints();

var identityEndpoint = identityApi.GetEndpoint(launchProfileName);

var catalogApi = builder.AddProject<BookWorm_Catalog>("catalog-api")
    .WithReference(blobs)
    .WithReference(rabbitMq)
    .WithReference(catalogDb)
    .WithReference(redis)
    .WithReference(openAi)
    .WithReference(blobs)
    .WaitFor(blobs)
    .WaitFor(rabbitMq)
    .WaitFor(catalogDb)
    .WaitFor(redis)
    .WithEnvironment("Identity__Url", identityEndpoint)
    .WithEnvironment("AiOptions__OpenAi__EmbeddingName", "text-embedding-3-small");

var orderingApi = builder.AddProject<BookWorm_Ordering>("ordering-api")
    .WithReference(rabbitMq)
    .WithReference(orderingDb)
    .WaitFor(rabbitMq)
    .WaitFor(orderingDb)
    .WithEnvironment("Identity__Url", identityEndpoint);

var ratingApi = builder.AddProject<BookWorm_Rating>("rating-api")
    .WithReference(rabbitMq)
    .WithReference(ratingDb)
    .WithReference(redis)
    .WaitFor(rabbitMq)
    .WaitFor(ratingDb)
    .WaitFor(redis)
    .WithEnvironment("Identity__Url", identityEndpoint);

var basketApi = builder.AddProject<BookWorm_Basket>("basket-api")
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WithEnvironment("Identity__Url", identityEndpoint);

var notificationApi = builder.AddProject<BookWorm_Notification>("notification-api")
    .WithReference(rabbitMq)
    .WithReference(notificationDb)
    .WithReference(smtpServer)
    .WaitFor(rabbitMq)
    .WaitFor(notificationDb)
    .WaitFor(smtpServer)
    .WithEnvironment("Smtp__Email", fromEmail);

var gateway = builder.AddProject<BookWorm_Gateway>("gateway")
    .WithReference(redis)
    .WaitFor(redis);

// Health checks
builder.AddHealthChecksUi("healthchecksui")
    .WithReference(gateway)
    .WithReference(identityApi)
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(ratingApi)
    .WithReference(basketApi)
    .WithReference(notificationApi)
    .WithExternalHttpEndpoints();

identityApi
    .WithEnvironment("Services__Catalog", catalogApi.GetEndpoint(launchProfileName))
    .WithEnvironment("Services__Ordering", orderingApi.GetEndpoint(launchProfileName))
    .WithEnvironment("Services__Rating", ratingApi.GetEndpoint(launchProfileName))
    .WithEnvironment("Services__Basket", basketApi.GetEndpoint(launchProfileName))
    .WithEnvironment("Services__Gateway", gateway.GetEndpoint(launchProfileName));

gateway
    .WithEnvironment("BFF__Authority", identityApi.GetEndpoint(launchProfileName))
    .WithEnvironment("BFF__Api__RemoteUrl", $"{catalogApi.GetEndpoint(launchProfileName)}/api/v1/authors");

builder.Build().Run();
