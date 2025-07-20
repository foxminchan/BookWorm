namespace BookWorm.AppHost.Extensions.Network;

public static class EndpointExtensions
{
    /// <summary>
    ///     Hides the plain HTTP link from the resource endpoints when the application is launched with an HTTPS profile.
    /// </summary>
    /// <param name="builder">
    ///     The distributed application builder to configure.
    /// </param>
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
