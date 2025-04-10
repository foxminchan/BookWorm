using Aspire.Hosting.Azure;
using Azure.Provisioning.CosmosDB;
using Azure.Provisioning.PostgreSql;
using Azure.Provisioning.Redis;
using Azure.Provisioning.Storage;
using BookWorm.Constants;
using Microsoft.Extensions.Hosting;
using RedisResource = Azure.Provisioning.Redis.RedisResource;

namespace BookWorm.AppHost;

public static class Extensions
{
    /// <summary>
    ///     Adds AI capabilities to the distributed application builder.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="resources">The resource builder for the project resource.</param>
    /// <returns>The updated distributed application builder.</returns>
    public static void AddAi(
        this IDistributedApplicationBuilder builder,
        List<IResourceBuilder<ProjectResource>?> resources
    )
    {
        var ollama = builder
            .AddOllama("ollama")
            .WithDataVolume()
            .WithGPUSupport()
            .WithOpenWebUI()
            .WithImagePullPolicy(ImagePullPolicy.Always)
            .WithLifetime(ContainerLifetime.Persistent)
            .PublishAsContainer();

        var embeddings = ollama.AddModel(Components.Ollama.Embedding, "nomic-embed-text:latest");

        var chat = ollama.AddModel(Components.Ollama.Chat, "deepseek-r1:1.5b");

        foreach (var resource in resources.Where(x => x is not null))
        {
            resource
                ?.WithReference(embeddings)
                .WaitFor(embeddings)
                .WithReference(chat)
                .WaitFor(chat);
        }
    }

    /// <summary>
    ///     Configures the email provider for the distributed application builder.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="notification">The resource builder for the project resource.</param>
    /// <param name="mailpit">The resource builder for the MailPit container resource.</param>
    public static void ConfigureEmailProvider(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ProjectResource> notification,
        IResourceBuilder<MailPitContainerResource> mailpit
    )
    {
        if (builder.Environment.IsDevelopment())
        {
            notification.WithReference(mailpit).WaitFor(mailpit);
            mailpit.WithParentRelationship(notification);
        }
        else
        {
            notification
                .WithEnvironment("SendGrid__ApiKey", builder.AddParameter("api-key", true))
                .WithEnvironment(
                    "SendGrid__SenderEmail",
                    builder.AddParameter("sender-email", true)
                )
                .WithEnvironment("SendGrid__SenderName", builder.AddParameter("sender-name", true));
        }
    }

    /// <summary>
    ///     Configures Azure resources for the distributed application builder.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="cosmos">The resource builder for Azure Cosmos DB.</param>
    /// <param name="storage">The resource builder for Azure Storage.</param>
    /// <param name="signalR">The resource builder for Azure SignalR.</param>
    /// <param name="postgres">The resource builder for Azure Postgres.</param>
    /// <param name="redis">The resource builder for Azure Redis.</param>
    public static void ConfigAzureResource(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<AzureCosmosDBResource> cosmos,
        IResourceBuilder<AzureStorageResource> storage,
        IResourceBuilder<AzureSignalRResource> signalR,
        IResourceBuilder<AzurePostgresFlexibleServerResource> postgres,
        IResourceBuilder<AzureRedisCacheResource> redis
    )
    {
        const string project = "Project";

        if (builder.Environment.IsDevelopment())
        {
            cosmos.RunAsPreviewEmulator(config =>
                config
                    .WithDataExplorer()
                    .WithDataVolume()
                    .WithImagePullPolicy(ImagePullPolicy.Always)
                    .WithLifetime(ContainerLifetime.Persistent)
            );

            storage.RunAsEmulator(config =>
                config
                    .WithDataVolume()
                    .WithImagePullPolicy(ImagePullPolicy.Always)
                    .WithLifetime(ContainerLifetime.Persistent)
            );

            signalR.RunAsEmulator(config =>
                config
                    .WithImagePullPolicy(ImagePullPolicy.Always)
                    .WithLifetime(ContainerLifetime.Persistent)
            );

            postgres.RunAsContainer(cfg =>
                cfg.WithPgAdmin()
                    .WithDataVolume()
                    .WithImagePullPolicy(ImagePullPolicy.Always)
                    .WithLifetime(ContainerLifetime.Persistent)
            );

            redis.RunAsContainer(config =>
                config
                    .WithRedisInsight()
                    .WithDataVolume()
                    .WithImagePullPolicy(ImagePullPolicy.Always)
                    .WithLifetime(ContainerLifetime.Persistent)
            );
        }

        cosmos.AddCosmosDatabase(Components.Database.Rating);

        cosmos
            .WithAccessKeyAuthentication()
            .ConfigureInfrastructure(infra =>
            {
                var cosmosDbAccount = infra
                    .GetProvisionableResources()
                    .OfType<CosmosDBAccount>()
                    .Single();

                cosmosDbAccount.Kind = CosmosDBAccountKind.GlobalDocumentDB;
                cosmosDbAccount.ConsistencyPolicy = new()
                {
                    DefaultConsistencyLevel = DefaultConsistencyLevel.Session,
                };
                cosmosDbAccount.Tags.Add(nameof(Environment), builder.Environment.EnvironmentName);
                cosmosDbAccount.Tags.Add(project, nameof(BookWorm));
            });

        storage.ConfigureInfrastructure(infra =>
        {
            var storageAccount = infra
                .GetProvisionableResources()
                .OfType<StorageAccount>()
                .Single();

            storageAccount.AccessTier = StorageAccountAccessTier.Hot;
            storageAccount.Sku = new() { Name = StorageSkuName.PremiumZrs };
            storageAccount.Tags.Add(nameof(Environment), builder.Environment.EnvironmentName);
            storageAccount.Tags.Add(project, nameof(BookWorm));
        });

        postgres
            .WithPasswordAuthentication()
            .ConfigureInfrastructure(infra =>
            {
                var flexibleServer = infra
                    .GetProvisionableResources()
                    .OfType<PostgreSqlFlexibleServer>()
                    .Single();

                flexibleServer.Sku = new()
                {
                    Tier = PostgreSqlFlexibleServerSkuTier.GeneralPurpose,
                };
                flexibleServer.Tags.Add(nameof(Environment), builder.Environment.EnvironmentName);
                flexibleServer.Tags.Add(project, nameof(BookWorm));
            });

        redis
            .WithAccessKeyAuthentication()
            .ConfigureInfrastructure(infra =>
            {
                var resource = infra.GetProvisionableResources().OfType<RedisResource>().Single();

                resource.Sku = new()
                {
                    Family = RedisSkuFamily.BasicOrStandard,
                    Name = RedisSkuName.Basic,
                    Capacity = 1,
                };
                resource.Tags.Add(nameof(Environment), builder.Environment.EnvironmentName);
                resource.Tags.Add(project, nameof(BookWorm));
            });
    }

    /// <summary>
    ///     Adds the project publisher to the distributed application builder.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    public static void AddProjectPublisher(this IDistributedApplicationBuilder builder)
    {
        builder.AddAzurePublisher();
        builder.AddKubernetesPublisher();
    }
}
