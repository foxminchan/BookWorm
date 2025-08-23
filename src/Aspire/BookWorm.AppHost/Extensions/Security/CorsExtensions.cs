using BookWorm.Constants.Core;

namespace BookWorm.AppHost.Extensions.Security;

public static class CorsExtensions
{
    /// <summary>
    ///     Configures Cross-Origin Resource Sharing (CORS) settings for the distributed application.
    ///     Adds environment variables for BackOffice and StoreFront URLs when in publish mode.
    /// </summary>
    /// <param name="builder">The distributed application builder instance to configure with CORS settings.</param>
    public static void ConfigureCors(this IDistributedApplicationBuilder builder)
    {
        builder.Eventing.Subscribe<BeforeStartEvent>(
            (e, _) =>
            {
                var backofficeParam = builder
                    .AddParameter("backoffice-domain", true)
                    .WithDescription(ParameterDescriptions.Cors.BackOfficeUrl, true)
                    .WithCustomInput(_ =>
                        new()
                        {
                            Label = "BackOffice Domain",
                            InputType = InputType.Text,
                            Value = "https://admin.bookworm.com",
                            Description = "Enter the BackOffice domain URL",
                        }
                    );

                var storefrontParam = builder
                    .AddParameter("storefront-domain", true)
                    .WithDescription(ParameterDescriptions.Cors.StoreFrontUrl, true)
                    .WithCustomInput(_ =>
                        new()
                        {
                            Label = "StoreFront Domain",
                            InputType = InputType.Text,
                            Value = "https://bookworm.com",
                            Description = "Enter the StoreFront domain URL",
                        }
                    );

                IResourceBuilder<ParameterResource>[] origins = [backofficeParam, storefrontParam];

                string[] methods =
                [
                    Restful.Methods.Get,
                    Restful.Methods.Post,
                    Restful.Methods.Put,
                    Restful.Methods.Patch,
                    Restful.Methods.Delete,
                    Restful.Methods.Options,
                ];

                string[] headers =
                [
                    "Content-Type",
                    "Authorization",
                    "X-Requested-With",
                    "Accept",
                    "Origin",
                    Restful.RequestIdHeader,
                ];

                foreach (var project in e.Model.Resources.OfType<ProjectResource>())
                {
                    var resourceBuilder = builder.CreateResourceBuilder(project);

                    foreach (var (origin, index) in origins.Select((o, i) => (o, i)))
                    {
                        resourceBuilder = resourceBuilder.WithEnvironment(
                            $"Cors__Origins__{index}",
                            origin
                        );
                    }

                    foreach (var (method, index) in methods.Select((m, i) => (m, i)))
                    {
                        resourceBuilder = resourceBuilder.WithEnvironment(
                            $"Cors__Methods__{index}",
                            method
                        );
                    }

                    foreach (var (header, index) in headers.Select((h, i) => (h, i)))
                    {
                        resourceBuilder = resourceBuilder.WithEnvironment(
                            $"Cors__Headers__{index}",
                            header
                        );
                    }

                    resourceBuilder = resourceBuilder
                        .WithEnvironment("Cors__MaxAge", (24 * 60 * 60).ToString())
                        .WithEnvironment("Cors__AllowCredentials", "true");

                    resourceBuilder.PublishAsAzureContainerApp(
                        (infra, app) =>
                        {
                            app.Configuration.Ingress.CorsPolicy.AllowedOrigins =
                            [
                                backofficeParam.AsProvisioningParameter(infra),
                                storefrontParam.AsProvisioningParameter(infra),
                            ];

                            app.Configuration.Ingress.CorsPolicy.AllowedMethods = [.. methods];
                            app.Configuration.Ingress.CorsPolicy.AllowedHeaders = [.. headers];
                            app.Configuration.Ingress.CorsPolicy.MaxAge = 24 * 60 * 60;
                            app.Configuration.Ingress.CorsPolicy.AllowCredentials = true;

                            app.Tags.Add(nameof(Environment), builder.Environment.EnvironmentName);
                            app.Tags.Add(nameof(Projects), nameof(BookWorm));
                        }
                    );
                }

                return Task.CompletedTask;
            }
        );
    }
}
