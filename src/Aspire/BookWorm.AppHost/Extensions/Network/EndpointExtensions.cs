namespace BookWorm.AppHost.Extensions.Network;

public static class EndpointExtensions
{
    /// <summary>
    ///     Hides the plain HTTP link from the resource endpoints when the application is launched with an HTTPS profile.
    /// </summary>
    /// <param name="builder">
    ///     The distributed application builder to configure.
    /// </param>
    /// <remarks>
    ///     This method improves security and user experience by hiding insecure HTTP endpoints in HTTPS environments:
    ///     - Only applies when the application is launched with an HTTPS profile
    ///     - Subscribes to the <see cref="BeforeStartEvent" /> for startup-time configuration
    ///     - Updates HTTP endpoint display location to <see cref="UrlDisplayLocation.DetailsOnly" /> for all project resources
    ///     - Ensures HTTP endpoints are still accessible but not prominently displayed in the main interface
    ///     - Encourages developers to use secure HTTPS connections by default
    ///     - Applies to all resources implementing <see cref="IResourceWithEndpoints" />
    /// </remarks>
    /// <example>
    ///     <code>
    ///     var builder = DistributedApplication.CreateBuilder(args);
    ///
    ///     builder.HidePlainHttpLink();
    ///
    ///     builder.Build().Run();
    ///     </code>
    /// </example>
    public static void HidePlainHttpLink(this IDistributedApplicationBuilder builder)
    {
        if (builder.IsHttpsLaunchProfile())
        {
            builder.Eventing.Subscribe<BeforeStartEvent>(
                (e, _) =>
                {
                    foreach (var r in e.Model.Resources.OfType<IResourceWithEndpoints>())
                    {
                        if (r is ProjectResource projectResource)
                        {
                            builder
                                .CreateResourceBuilder(projectResource)
                                .WithUrls(c =>
                                {
                                    c.Urls.Where(u => u.Endpoint?.EndpointName == Protocol.Http)
                                        .ToList()
                                        .ForEach(u =>
                                            u.DisplayLocation = UrlDisplayLocation.DetailsOnly
                                        );
                                });
                        }
                    }

                    return Task.CompletedTask;
                }
            );
        }
    }

    /// <summary>
    ///     Adds an AsyncAPI UI endpoint to the project resource for API documentation and testing.
    /// </summary>
    /// <param name="builder">The project resource builder to configure with AsyncAPI UI.</param>
    /// <returns>The updated project resource builder with AsyncAPI UI endpoint configured.</returns>
    /// <remarks>
    ///     This method configures an AsyncAPI UI endpoint with the following features:
    ///     - Creates a dedicated endpoint at <c>/asyncapi/ui</c> for interactive API documentation
    ///     - Uses the current launch profile name to determine the appropriate endpoint
    ///     - Displays with uppercase endpoint name formatting for visual consistency
    ///     - Sets display location to show in both summary and details views for easy access
    ///     - Provides interactive interface for testing asynchronous API operations
    ///     - Integrates with the project's existing endpoint configuration
    /// </remarks>
    /// <example>
    ///     <code>
    ///     builder.AddProject&lt;WebApi&gt;("api")
    ///            .WithAsyncAPIUI();
    ///     </code>
    /// </example>
    public static IResourceBuilder<ProjectResource> WithAsyncAPIUI(
        this IResourceBuilder<ProjectResource> builder
    )
    {
        if (!builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            return builder;
        }

        var endpointName = builder.ApplicationBuilder.GetLaunchProfileName();

        return builder.WithUrlForEndpoint(
            endpointName,
            _ =>
                new()
                {
                    Url = "/asyncapi/ui",
                    DisplayText = $"Async API ({endpointName.ToUpperInvariant()})",
                    DisplayLocation = UrlDisplayLocation.SummaryAndDetails,
                }
        );
    }
}
