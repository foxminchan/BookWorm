using BookWorm.Scalar;

namespace BookWorm.AppHost.Extensions;

public static class ProjectExtensions
{
    /// <summary>
    ///     Adds project-specific publishers to the distributed application builder.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    public static void AddProjectPublisher(this IDistributedApplicationBuilder builder)
    {
        builder.AddAzureContainerAppEnvironment(nameof(BookWorm).ToLower());
        builder.AddAzurePublisher();
        builder.AddKubernetesPublisher();
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
