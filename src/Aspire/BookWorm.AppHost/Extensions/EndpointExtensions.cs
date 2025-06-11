namespace BookWorm.AppHost.Extensions;

public static class EndpointExtensions
{
    /// <summary>
    ///     Hides the plain HTTP link from the resource endpoints when the application is launched with an HTTPS profile.
    /// </summary>
    /// <param name="builder">
    ///     The distributed application builder to configure.
    /// </param>
    /// <remarks>
    ///     This method subscribes to the <see cref="BeforeStartEvent"/> and updates the display location of HTTP endpoints
    ///     to <see cref="UrlDisplayLocation.DetailsOnly"/> for all resources of type <see cref="IResourceWithEndpoints"/>.
    ///     This ensures that
    /// </remarks>
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
    ///     Configures the resource builder to update the display text for HTTP and HTTPS endpoints
    ///     with the specified template for Open API.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the resource, which must inherit from <see cref="ProjectResource" />.
    /// </typeparam>
    /// <param name="builder">
    ///     The resource builder to configure.
    /// </param>
    /// <returns>
    ///     The configured <see cref="IResourceBuilder{T}" /> instance.
    /// </returns>
    /// <remarks>
    ///     This method checks if the application is running in the execution context's run mode.
    ///     If so, it updates the display text for HTTP and HTTPS endpoints using the provided template.
    /// </remarks>
    public static IResourceBuilder<T> WithOpenApi<T>(this IResourceBuilder<T> builder)
        where T : ProjectResource
    {
        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            builder.UpdateHttpAndHttpsEndpoints("Open API ({0})");
        }

        return builder;
    }

    /// <summary>
    ///     Configures the resource builder to add health check endpoints to the resource representation.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the resource, which must inherit from <see cref="ProjectResource" />.
    /// </typeparam>
    /// <param name="builder">
    ///     The resource builder to configure.
    /// </param>
    /// <returns>
    ///     The configured <see cref="IResourceBuilder{T}" /> instance.
    /// </returns>
    /// <remarks>
    ///     This method adds two health check endpoints:
    ///     1. A "healthchecks" endpoint displayed only in the details view.
    ///     2. A "/health" endpoint with a display text of "Health Checks", displayed only in the details view.
    ///     The protocol for the "/health" endpoint is determined based on whether the application is running with an HTTPS
    ///     launch profile.
    /// </remarks>
    public static IResourceBuilder<T> WithHealthCheck<T>(this IResourceBuilder<T> builder)
        where T : ProjectResource
    {
        builder
            .WithUrlForEndpoint(
                "healthchecks",
                url => url.DisplayLocation = UrlDisplayLocation.DetailsOnly
            )
            .WithUrlForEndpoint(
                builder.ApplicationBuilder.IsHttpsLaunchProfile() ? Protocol.Https : Protocol.Http,
                _ =>
                    new()
                    {
                        Url = "/health",
                        DisplayText = "Health Checks",
                        DisplayLocation = UrlDisplayLocation.DetailsOnly,
                    }
            );

        return builder;
    }

    /// <summary>
    ///     Configures the resource builder to add an Async API endpoint to the resource representation.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the resource, which must inherit from <see cref="ProjectResource" />.
    /// </typeparam>
    /// <param name="builder">
    ///     The resource builder to configure.
    /// </param>
    /// <param name="isOnlyAsyncApi">
    ///     A boolean indicating whether only the Async API endpoint should be updated.
    ///     If <c>true</c>, the display text for HTTP and HTTPS endpoints is updated with "Async API ({0})".
    ///     If <c>false</c>, an Async API UI endpoint is added.
    /// </param>
    /// <returns>
    ///     The configured <see cref="IResourceBuilder{T}" /> instance.
    /// </returns>
    /// <remarks>
    ///     This method checks if the application is running in the execution context's run mode.
    ///     If <paramref name="isOnlyAsyncApi" /> is <c>true</c>, it updates the display text for HTTP and HTTPS endpoints.
    ///     Otherwise, it adds an Async API UI endpoint with a display text based on the protocol.
    /// </remarks>
    public static IResourceBuilder<T> WithAsyncApi<T>(
        this IResourceBuilder<T> builder,
        bool isOnlyAsyncApi = false
    )
        where T : ProjectResource
    {
        if (!builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            return builder;
        }

        if (isOnlyAsyncApi)
        {
            builder.UpdateHttpAndHttpsEndpoints("Async API ({0})");
        }
        else
        {
            var endpointName = builder.ApplicationBuilder.IsHttpsLaunchProfile()
                ? Protocol.Https
                : Protocol.Http;

            builder.WithUrlForEndpoint(
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

        return builder;
    }

    private static void UpdateHttpAndHttpsEndpoints<T>(
        this IResourceBuilder<T> builder,
        string displayTextTemplate
    )
        where T : ProjectResource
    {
        builder.WithUrls(c =>
            c.Urls.Where(u =>
                    u.Endpoint?.EndpointName == Protocol.Http
                    || u.Endpoint?.EndpointName == Protocol.Https
                )
                .ToList()
                .ForEach(u =>
                    u.DisplayText = string.Format(
                        displayTextTemplate,
                        u.Endpoint?.EndpointName.ToUpperInvariant()
                    )
                )
        );
    }
}
