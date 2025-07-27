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
}
