using Aspire.Hosting.Yarp;
using BookWorm.Constants.Aspire;
using BookWorm.Scalar;

namespace BookWorm.AppHost.Extensions;

public static class ProjectExtensions
{
    /// <summary>
    ///     Configures the resource builder to use the Scalar API documentation if not in publish mode.
    /// </summary>
    /// <param name="builder">The resource builder for the project resource.</param>
    /// <returns>The updated resource builder.</returns>
    public static IResourceBuilder<ProjectResource> WithScalarApiDocs(
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
        IResourceBuilder<YarpResource> entryPoint
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
                .WithReference(entryPoint.Resource.GetEndpoint("http"))
                .WaitFor(entryPoint);
        }
    }
}
