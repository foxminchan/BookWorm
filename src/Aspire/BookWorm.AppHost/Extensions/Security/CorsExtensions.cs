using BookWorm.Constants.Core;

namespace BookWorm.AppHost.Extensions.Security;

public static class CorsExtensions
{
    /// <summary>
    ///     Configures Cross-Origin Resource Sharing (CORS) settings for the distributed application.
    ///     Adds environment variables for BackOffice and StoreFront URLs when in publish mode.
    /// </summary>
    /// <param name="builder">The distributed application builder instance.</param>
    public static void ConfigureCors(this IDistributedApplicationBuilder builder)
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
                        .WithEnvironment("Cors__StoreFrontUrl", storefrontParam)
                        .PublishAsAzureContainerApp(
                            (infra, app) =>
                            {
                                app.Configuration.Ingress.CorsPolicy.AllowedOrigins =
                                [
                                    backofficeParam.AsProvisioningParameter(infra),
                                    storefrontParam.AsProvisioningParameter(infra),
                                ];

                                app.Configuration.Ingress.CorsPolicy.AllowedMethods =
                                [
                                    Restful.Methods.Get,
                                    Restful.Methods.Post,
                                    Restful.Methods.Put,
                                    Restful.Methods.Patch,
                                    Restful.Methods.Delete,
                                    Restful.Methods.Options,
                                ];

                                app.Tags.Add(
                                    nameof(Environment),
                                    builder.Environment.EnvironmentName
                                );
                                app.Tags.Add(nameof(Projects), nameof(BookWorm));
                            }
                        );
                }

                return Task.CompletedTask;
            }
        );
    }
}
