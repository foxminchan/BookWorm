using Aspire.Hosting.Yarp;

namespace BookWorm.AppHost.Extensions.Frontend;

internal static class FrontendExtensions
{
    private const string AppUrlEnvironmentVariable = "NEXT_PUBLIC_APP_URL";
    private const string BuildStage = "build";

    extension(IDistributedApplicationBuilder builder)
    {
        public (
            IResourceBuilder<IResourceWithEndpoints> Storefront,
            IResourceBuilder<IResourceWithEndpoints> Backoffice
        ) AddFrontendApps(
            IResourceBuilder<YarpResource> gateway,
            IResourceBuilder<EntraIdApplicationResource> entraId
        )
        {
            return builder.ExecutionContext.IsRunMode
                ? AddTurborepoApps(builder, gateway, entraId)
                : AddPublishedContainerApps(builder, gateway, entraId);
        }
    }

    private static (
        IResourceBuilder<IResourceWithEndpoints> Storefront,
        IResourceBuilder<IResourceWithEndpoints> Backoffice
    ) AddTurborepoApps(
        IDistributedApplicationBuilder builder,
        IResourceBuilder<YarpResource> gateway,
        IResourceBuilder<EntraIdApplicationResource> entraId
    )
    {
        var turbo = builder
            .AddTurborepoApp(
                Components.TurboRepo,
                Path.GetFullPath("../../Clients", builder.AppHostDirectory)
            )
            .WithBun(true)
            .WithPackageManagerLaunch();

        var storefront = turbo
            .AddApp(Clients.StoreFront, Clients.StoreFrontTurboApp)
            .WithOtlpExporter()
            .WithHttpEndpoint(env: "PORT")
            .WithMappedEndpointPort()
            .WithHttpHealthCheck()
            .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTPS", gateway.GetEndpoint(Uri.UriSchemeHttps))
            .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTP", gateway.GetEndpoint(Uri.UriSchemeHttp))
            .WithEnvironment("NEXT_PUBLIC_COPILOT_ENABLED", "true")
            .WithEntraId(entraId, gateway);

        storefront.WithEnvironment(
            AppUrlEnvironmentVariable,
            gateway.GetEndpoint(Uri.UriSchemeHttps)
        );

        var backoffice = turbo
            .AddApp(Clients.BackOffice, Clients.BackOfficeTurboApp)
            .WithOtlpExporter()
            .WithHttpEndpoint(env: "PORT")
            .WithMappedEndpointPort()
            .WithHttpHealthCheck()
            .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTPS", gateway.GetEndpoint(Uri.UriSchemeHttps))
            .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTP", gateway.GetEndpoint(Uri.UriSchemeHttp))
            .WithEntraId(entraId, gateway, "/backoffice");

        backoffice.WithEnvironment(
            AppUrlEnvironmentVariable,
            ReferenceExpression.Create($"{gateway.GetEndpoint(Uri.UriSchemeHttps)}/backoffice")
        );

        return (storefront, backoffice);
    }

    private static (
        IResourceBuilder<IResourceWithEndpoints> Storefront,
        IResourceBuilder<IResourceWithEndpoints> Backoffice
    ) AddPublishedContainerApps(
        IDistributedApplicationBuilder builder,
        IResourceBuilder<YarpResource> gateway,
        IResourceBuilder<EntraIdApplicationResource> entraId
    )
    {
        var storefront = ConfigurePublishedFrontend(
            AddFrontendContainer(
                builder,
                Clients.StoreFront,
                Clients.StoreFrontTurboApp,
                "@bookworm/storefront",
                true
            ),
            gateway,
            entraId,
            true
        );

        storefront.WithEnvironment(
            AppUrlEnvironmentVariable,
            gateway.GetEndpoint(Uri.UriSchemeHttps)
        );

        var backoffice = ConfigurePublishedFrontend(
            AddFrontendContainer(
                builder,
                Clients.BackOffice,
                Clients.BackOfficeTurboApp,
                "@bookworm/backoffice"
            ),
            gateway,
            entraId,
            pathPrefix: "/backoffice"
        );

        backoffice.WithEnvironment(
            AppUrlEnvironmentVariable,
            ReferenceExpression.Create($"{gateway.GetEndpoint(Uri.UriSchemeHttps)}/backoffice")
        );

        return (storefront, backoffice);
    }

    private static IResourceBuilder<ContainerResource> ConfigurePublishedFrontend(
        IResourceBuilder<ContainerResource> frontend,
        IResourceBuilder<YarpResource> gateway,
        IResourceBuilder<EntraIdApplicationResource> entraId,
        bool includeCopilot = false,
        string? pathPrefix = null
    )
    {
        frontend
            .WithOtlpExporter()
            .WithHttpEndpoint(targetPort: 3000, env: "PORT")
            .WithHttpHealthCheck()
            .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTPS", gateway.GetEndpoint(Uri.UriSchemeHttps))
            .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTP", gateway.GetEndpoint(Uri.UriSchemeHttp));

        if (includeCopilot)
        {
            frontend.WithEnvironment("NEXT_PUBLIC_COPILOT_ENABLED", "true");
        }

        return frontend.WithEntraId(entraId, gateway, pathPrefix);
    }

    private static IResourceBuilder<ContainerResource> AddFrontendContainer(
        IDistributedApplicationBuilder builder,
        string name,
        string appName,
        string turboFilter,
        bool includeCopilot = false
    )
    {
        var clientsPath = Path.GetFullPath("../../Clients", builder.AppHostDirectory);

        return builder.AddDockerfileBuilder(
            name,
            clientsPath,
            context =>
            {
                var buildStage = context
                    .Builder.From("oven/bun:1", BuildStage)
                    .WorkDir("/app")
                    .Copy(".", ".");

                if (includeCopilot)
                {
                    buildStage.Env("NEXT_PUBLIC_COPILOT_ENABLED", "true");
                }

                buildStage
                    .Run("bun install --frozen-lockfile")
                    .Run($"bunx turbo build --filter={turboFilter}");

                context
                    .Builder.From("oven/bun:1-alpine", "runtime")
                    .WorkDir("/app")
                    .CopyFrom(BuildStage, $"/app/apps/{appName}/.next/standalone", ".")
                    .CopyFrom(
                        BuildStage,
                        $"/app/apps/{appName}/.next/static",
                        $"./apps/{appName}/.next/static"
                    )
                    .CopyFrom(BuildStage, $"/app/apps/{appName}/public", $"./apps/{appName}/public")
                    .Env("NODE_ENV", "production")
                    .Env("NEXT_TELEMETRY_DISABLED", "1")
                    .Env("HOSTNAME", "0.0.0.0")
                    .Expose(3000)
                    .Entrypoint(["bun", $"apps/{appName}/server.js"]);

                return Task.CompletedTask;
            },
            "runtime"
        );
    }
}
