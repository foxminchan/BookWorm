using BookWorm.Scalar;

namespace BookWorm.AppHost.Extensions;

public static class ProjectExtensions
{
    /// <summary>
    ///     Adds project-specific publishers to the distributed application builder.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <remarks>
    ///     Starting in Aspire 9.2, we can use the new DockerComposePublisher to generate a docker-compose file.
    ///     Run 'dotnet run --publisher kubernetes --output-path .\deploys\helm --project
    ///     .\src\BookWorm.AppHost\BookWorm.AppHost.csproj' to publish as a helm chart.
    ///     Run 'dotnet run --publisher azure --output-path .\deploys\bicep --project
    ///     .\src\BookWorm.AppHost\BookWorm.AppHost.csproj' to publish as a bicep template.
    /// </remarks>
    public static void AddProjectPublisher(this IDistributedApplicationBuilder builder)
    {
        builder.AddAzurePublisher();
        builder.AddKubernetesPublisher();
    }

    /// <summary>
    ///     Adds an Azure Container App Environment to the distributed application.
    /// </summary>
    /// <param name="builder">The distributed application builder to configure.</param>
    /// <remarks>
    ///     This method creates a container app environment with the current environment name,
    ///     configures Azure Developer CLI (azd) compatible resource naming,
    ///     and provisions it as a service in the application.
    /// </remarks>
    public static void AddAzureContainerAppEnvironment(this IDistributedApplicationBuilder builder)
    {
        var environmentName = $"{nameof(BookWorm).ToLower()}-aca";

        builder
            .AddAzureContainerAppEnvironment(environmentName)
            .WithAzdResourceNaming()
            .ProvisionAsService();
    }

    /// <summary>
    ///     Configures the resource builder to use the Scalar API client if not in publish mode.
    /// </summary>
    /// <param name="builder">The resource builder for the project resource.</param>
    /// <returns>The updated resource builder.</returns>
    public static IResourceBuilder<ProjectResource> WithScalarApiClient(
        this IResourceBuilder<ProjectResource> builder
    )
    {
        if (builder.ApplicationBuilder.ExecutionContext.IsPublishMode)
        {
            return builder;
        }

        builder.WithScalar();

        return builder;
    }
}
