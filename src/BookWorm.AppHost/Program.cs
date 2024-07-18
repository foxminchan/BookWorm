using BookWorm.AppHost;
using BookWorm.HealthCheck.Hosting;
using BookWorm.MailDev.Hosting;
using BookWorm.Swagger.Hosting;
using Microsoft.Extensions.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddForwardedHeaders();

var postgresUser = builder.AddParameter("SqlUser", true);
var postgresPassword = builder.AddParameter("SqlPassword", true);
var rabbitMqPassword = builder.AddParameter("RabbitMqPassword", true);

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
    .WithDataBindMount("../../mnt/redis");

var catalogDb = postgres.AddDatabase("catalogdb");
var orderingDb = postgres.AddDatabase("orderingdb");
var identityDb = postgres.AddDatabase("identitydb");
var notificationDb = postgres.AddDatabase("notificationdb");
var ratingDb = mongodb.AddDatabase("ratingdb");

var storage = builder.AddAzureStorage("storage");

if (builder.Environment.IsDevelopment())
{
    storage.RunAsEmulator(config => config.WithDataBindMount("../../mnt/azurite"));
}

var blobs = storage.AddBlobs("blobs");

var openAi = builder.AddConnectionString("openai");

var rabbitMq = builder
    .AddRabbitMQ("eventbus", rabbitMqPassword)
    .WithManagementPlugin();

var smtpServer = builder.AddMailDev("mailserver", 1080);

// Services
var identityApi = builder.AddProject<BookWorm_Identity>("identity-api")
    .WithExternalHttpEndpoints()
    .WithReference(identityDb);

var identityEndpoint = identityApi.GetEndpoint(launchProfileName);

var catalogApi = builder.AddProject<BookWorm_Catalog>("catalog-api")
    .WithReference(blobs)
    .WithReference(rabbitMq)
    .WithReference(catalogDb)
    .WithReference(openAi)
    .WithEnvironment("Identity__Url", identityEndpoint)
    .WithEnvironment("AiOptions__OpenAi__EmbeddingName", "text-embedding-3-small")
    .WithEnvironment("AzuriteOptions__ConnectionString", blobs.WithEndpoint())
    .WithSwaggerUi(endpointName: "https");

var orderingApi = builder.AddProject<BookWorm_Ordering>("ordering-api")
    .WithReference(rabbitMq)
    .WithReference(orderingDb)
    .WithEnvironment("Identity__Url", identityEndpoint)
    .WithSwaggerUi();

var ratingApi = builder.AddProject<BookWorm_Rating>("rating-api")
    .WithReference(rabbitMq)
    .WithReference(ratingDb)
    .WithEnvironment("Identity__Url", identityEndpoint)
    .WithSwaggerUi();

var basketApi = builder.AddProject<BookWorm_Basket>("basket-api")
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithEnvironment("Identity__Url", identityEndpoint)
    .WithSwaggerUi();

var notificationApi = builder.AddProject<BookWorm_Notification>("notification-api")
    .WithReference(rabbitMq)
    .WithReference(notificationDb)
    .WithReference(smtpServer);

// Reverse proxy
var bff = builder.AddProject<BookWorm_Web_Bff>("bff")
    .WithExternalHttpEndpoints()
    .WithEnvironment("BFF__Authority", identityEndpoint)
    .WithEnvironment(ResourceTemplate("catalog"), catalogApi.GetEndpoint(launchProfileName))
    .WithEnvironment(ResourceTemplate("ordering"), orderingApi.GetEndpoint(launchProfileName))
    .WithEnvironment(ResourceTemplate("rating"), ratingApi.GetEndpoint(launchProfileName))
    .WithEnvironment(ResourceTemplate("basket"), basketApi.GetEndpoint(launchProfileName));

// Web
var storeFront = builder.AddProject<BookWorm_StoreFront>("storefront")
    .WithExternalHttpEndpoints()
    .WithEnvironment("BFF__Url", bff.GetEndpoint(launchProfileName));

var backOffice = builder
    .AddExecutable("backoffice", "bun", "../BookWorm.Backoffice", "dev")
    .WithHttpEndpoint(3000, env: "PORT")
    .WithEnvironment("BROWSER", "none")
    .PublishAsDockerFile();

// Health checks
builder.AddHealthChecksUi("healthchecksui")
    .WithReference(identityApi)
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(ratingApi)
    .WithReference(basketApi)
    .WithReference(notificationApi)
    .WithReference(storeFront)
    .WithReference(bff)
    .WithExternalHttpEndpoints();

identityApi
    .WithEnvironment("Services__Bff", bff.GetEndpoint(launchProfileName))
    .WithEnvironment("Services__StoreFront", storeFront.GetEndpoint(launchProfileName))
    .WithEnvironment("Services__BackOffice", backOffice.GetEndpoint("http"))
    .WithEnvironment("Services__Catalog", catalogApi.GetEndpoint(launchProfileName))
    .WithEnvironment("Services__Ordering", orderingApi.GetEndpoint(launchProfileName))
    .WithEnvironment("Services__Rating", ratingApi.GetEndpoint(launchProfileName))
    .WithEnvironment("Services__Basket", basketApi.GetEndpoint(launchProfileName));

builder.Build().Run();
return;

static string ResourceTemplate(string service) =>
    $"ReverseProxy__Clusters__{service}__Destinations__{service}__Address";
