namespace BookWorm.AppHost.Extensions.Network;

public static class CorsExtensions
{
    /// <summary>
    ///     Configures Cross-Origin Resource Sharing (CORS) settings for the distributed application.
    ///     Adds environment variables for BackOffice and StoreFront URLs when in publish mode.
    /// </summary>
    /// <param name="builder">The distributed application builder instance.</param>
    public static void ConfigureCors(this IDistributedApplicationBuilder builder)
    {
        if (builder.ExecutionContext.IsPublishMode)
        {
            builder.Eventing.Subscribe<BeforeStartEvent>(
                (e, _) =>
                {
                    var backofficeParam = builder.AddParameter("backoffice-domain", true);
                    var storefrontParam = builder.AddParameter("storefront-domain", true);

                    foreach (var project in e.Model.Resources.OfType<ProjectResource>())
                    {
                        builder
                            .CreateResourceBuilder(project)
                            .WithEnvironment("Cors__BackOfficeUrl", backofficeParam)
                            .WithEnvironment("Cors__StoreFrontUrl", storefrontParam);
                    }
                    return Task.CompletedTask;
                }
            );
        }
    }
}
