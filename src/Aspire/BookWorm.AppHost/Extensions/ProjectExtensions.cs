using BookWorm.Constants;
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
    ///     .\src\Aspire\BookWorm.AppHost\BookWorm.AppHost.csproj' to publish as a helm chart.
    ///     Run 'dotnet run --publisher azure --output-path .\deploys\bicep --project
    ///     .\src\Aspire\BookWorm.AppHost\BookWorm.AppHost.csproj' to publish as a bicep template.
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
        var environmentName = $"{nameof(BookWorm).ToLowerInvariant()}-aca";

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

    /// <summary>
    ///     Adds K6 load testing to the distributed application.
    /// </summary>
    /// <param name="builder">The distributed application builder to configure.</param>
    /// <param name="entryPoint">The resource builder for the project resource to test.</param>
    /// <remarks>
    ///     This method configures a K6 load testing instance that:
    ///     - Mounts scripts from a Container/scripts directory
    ///     - Mounts reports to Container/reports directory
    ///     - Runs the main.js script with a random number of virtual users (10-100)
    ///     - References and waits for the specified entryPoint resource
    ///     - Only runs in run mode, not in publish mode
    /// </remarks>
    public static void AddK6(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ProjectResource> entryPoint
    )
    {
        if (builder.ExecutionContext.IsRunMode)
        {
            builder
                .AddK6(Components.K6)
                .WithImagePullPolicy(ImagePullPolicy.Always)
                .WithBindMount("Container/scripts", "/scripts", true)
                .WithBindMount("Container/dist", "/home/k6")
                .WithScript("/scripts/main.js", Random.Shared.Next(10, 100))
                .WithReference(entryPoint)
                .WaitFor(entryPoint);
        }
    }
}
