using BookWorm.Constants.Core;

namespace BookWorm.AppHost.Extensions.Security;

public static class CorsExtensions
{
    /// <summary>
    ///     Configures Cross-Origin Resource Sharing (CORS) settings for the distributed application.
    ///     Adds environment variables for BackOffice and StoreFront URLs when in publish mode.
    /// </summary>
    /// <param name="builder">The distributed application builder instance to configure with CORS settings.</param>
    /// <remarks>
    ///     This method provides comprehensive CORS configuration for multi-frontend applications:
    ///     - Subscribes to <see cref="BeforeStartEvent" /> for startup-time configuration
    ///     - Creates secure parameters for backoffice and storefront domain URLs (marked as secrets)
    ///     - Applies environment variables to all project resources for runtime CORS configuration
    ///     - Configures Azure Container App ingress CORS policies with:
    ///     - Allowed origins from backoffice and storefront domain parameters
    ///     - Standard RESTful HTTP methods (GET, POST, PUT, PATCH, DELETE, OPTIONS)
    ///     - Environment and project tags for resource management
    ///     - Enables secure cross-origin requests between different frontend applications and APIs
    ///     - Supports both development and production deployment scenarios
    /// </remarks>
    /// <example>
    ///     <code>
    ///     var builder = DistributedApplication.CreateBuilder(args);
    ///
    ///     builder.ConfigureCors();
    ///
    ///     builder.Build().Run();
    ///     </code>
    /// </example>
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
