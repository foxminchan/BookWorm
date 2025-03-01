using BookWorm.AppHost;
using BookWorm.Constants;
using BookWorm.HealthChecksUI;
using BookWorm.Scalar;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgresUser = builder.AddParameter("sql-user", true);
var postgresPassword = builder.AddParameter("sql-password", true);

var postgres = builder
    .AddPostgres("postgres", postgresUser, postgresPassword, 5432)
    .WithPgAdmin()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var redis = builder
    .AddRedis(Components.Redis, 6379)
    .WithRedisInsight()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var qdrant = builder
    .AddQdrant(Components.VectorDb)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var queue = builder
    .AddRabbitMQ(Components.Queue)
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint("tcp", e => e.Port = 5672)
    .WithEndpoint("management", e => e.Port = 15672);

var cosmos = builder.AddAzureCosmosDB(Components.Cosmos);
var storage = builder.AddAzureStorage("storage");

builder.ConfigAzureResource(cosmos, storage);

var blobStorage = storage.AddBlobs(Components.Blob);
var catalogDb = postgres.AddDatabase(Components.Database.Catalog);
var orderingDb = postgres.AddDatabase(Components.Database.Ordering);
var financeDb = postgres.AddDatabase(Components.Database.Finance);

var keycloak = builder
    .AddKeycloak(Components.KeyCloak, 8084)
    .WithDataVolume()
    .WithRealmImport("./realm-export.json");

var mailpit = builder.AddMailPit(Components.MailPit);

var catalogApi = builder
    .AddProject<BookWorm_Catalog>("bookworm-catalog")
    .WithScalar()
    .WithReplicas(2)
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
    .WaitFor(redis);
qdrant.WithParentRelationship(catalogApi);

var basketApi = builder
    .AddProject<BookWorm_Basket>("bookworm-basket")
    .WithScalar()
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(keycloak)
    .WaitFor(keycloak);

var notificationApi = builder
    .AddProject<BookWorm_Notification>("bookworm-notification")
    .WithReference(queue)
    .WaitFor(queue);

builder.ConfigureEmailProvider(notificationApi, mailpit);

var orderingApi = builder
    .AddProject<BookWorm_Ordering>("bookworm-ordering")
    .WithScalar()
    .WithReference(orderingDb)
    .WaitFor(orderingDb)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(redis)
    .WaitFor(redis);

var ratingApi = builder
    .AddProject<BookWorm_Rating>("bookworm-rating")
    .WithScalar()
    .WithReference(cosmos)
    .WaitFor(cosmos)
    .WithReference(queue)
    .WaitFor(queue)
    .WithReference(keycloak)
    .WaitFor(keycloak);

var financeApi = builder
    .AddProject<BookWorm_Finance>("bookworm-finance")
    .WithScalar()
    .WithReference(financeDb)
    .WaitFor(financeDb)
    .WithReference(queue)
    .WaitFor(queue);

var gateway = builder
    .AddProject<BookWorm_Gateway>("bookworm-gateway")
    .WithReference(catalogApi)
    .WaitFor(catalogApi)
    .WithReference(orderingApi)
    .WaitFor(orderingApi)
    .WithReference(ratingApi)
    .WaitFor(ratingApi)
    .WithReference(basketApi)
    .WaitFor(basketApi)
    .WithReference(financeApi)
    .WaitFor(financeApi)
    .WithReference(keycloak)
    .WaitFor(keycloak);

builder.AddAi(catalogApi);

builder
    .AddHealthChecksUi("healthchecksui")
    .WithReference(gateway)
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(ratingApi)
    .WithReference(basketApi)
    .WithReference(notificationApi)
    .WithReference(financeApi)
    .WithExternalHttpEndpoints();

builder.Build().Run();
