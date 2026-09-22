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
            IResourceBuilder<IResource> keycloak
        )
        {
            if (builder.ExecutionContext.IsRunMode)
            {
                return AddTurborepoApps(builder, gateway, keycloak);
            }

            return AddPublishedContainerApps(builder, gateway, keycloak);
        }
    }

    private static (
        IResourceBuilder<IResourceWithEndpoints> Storefront,
        IResourceBuilder<IResourceWithEndpoints> Backoffice
    ) AddTurborepoApps(
        IDistributedApplicationBuilder builder,
        IResourceBuilder<YarpResource> gateway,
        IResourceBuilder<IResource> keycloak
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
            .WithExternalHttpEndpoints()
            .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTPS", gateway.GetEndpoint(Uri.UriSchemeHttps))
            .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTP", gateway.GetEndpoint(Uri.UriSchemeHttp))
            .WithEnvironment("NEXT_PUBLIC_COPILOT_ENABLED", "true")
            .WaitFor(gateway)
            .WithKeycloak(keycloak);

        storefront.WithEnvironment(
            AppUrlEnvironmentVariable,
            storefront.GetEndpoint(Uri.UriSchemeHttp)
        );

        var backoffice = turbo
            .AddApp(Clients.BackOffice, Clients.BackOfficeTurboApp)
            .WithOtlpExporter()
            .WithHttpEndpoint(env: "PORT")
            .WithMappedEndpointPort()
            .WithHttpHealthCheck()
            .WithExternalHttpEndpoints()
            .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTPS", gateway.GetEndpoint(Uri.UriSchemeHttps))
            .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTP", gateway.GetEndpoint(Uri.UriSchemeHttp))
            .WaitFor(gateway)
            .WithKeycloak(keycloak);

        backoffice.WithEnvironment(
            AppUrlEnvironmentVariable,
            backoffice.GetEndpoint(Uri.UriSchemeHttp)
        );

        return (storefront, backoffice);
    }

    private static (
        IResourceBuilder<IResourceWithEndpoints> Storefront,
        IResourceBuilder<IResourceWithEndpoints> Backoffice
    ) AddPublishedContainerApps(
        IDistributedApplicationBuilder builder,
        IResourceBuilder<YarpResource> gateway,
        IResourceBuilder<IResource> keycloak
    )
    {
        var storefront = ConfigurePublishedFrontend(
            AddFrontendContainer(
                builder,
                Clients.StoreFront,
                Clients.StoreFrontTurboApp,
                "@bookworm/storefront"
            ),
            gateway,
            keycloak,
            true
        );

        storefront.WithEnvironment(
            AppUrlEnvironmentVariable,
            storefront.GetEndpoint(Uri.UriSchemeHttp)
        );

        var backoffice = ConfigurePublishedFrontend(
            AddFrontendContainer(
                builder,
                Clients.BackOffice,
                Clients.BackOfficeTurboApp,
                "@bookworm/backoffice"
            ),
            gateway,
            keycloak
        );

        backoffice.WithEnvironment(
            AppUrlEnvironmentVariable,
            backoffice.GetEndpoint(Uri.UriSchemeHttp)
        );

        return (storefront, backoffice);
    }

    private static IResourceBuilder<ContainerResource> ConfigurePublishedFrontend(
        IResourceBuilder<ContainerResource> frontend,
        IResourceBuilder<YarpResource> gateway,
        IResourceBuilder<IResource> keycloak,
        bool includeCopilot = false
    )
    {
        frontend
            .WithOtlpExporter()
            .WithHttpEndpoint(targetPort: 3000, env: "PORT")
            .WithHttpHealthCheck()
            .WithExternalHttpEndpoints()
            .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTPS", gateway.GetEndpoint(Uri.UriSchemeHttps))
            .WithEnvironment("NEXT_PUBLIC_GATEWAY_HTTP", gateway.GetEndpoint(Uri.UriSchemeHttp))
            .WaitFor(gateway);

        if (includeCopilot)
        {
            frontend.WithEnvironment("NEXT_PUBLIC_COPILOT_ENABLED", "true");
        }

        return KeycloakExtensions.ConfigureContainerKeycloak(frontend, keycloak);
    }

    private static IResourceBuilder<ContainerResource> AddFrontendContainer(
        IDistributedApplicationBuilder builder,
        string name,
        string appName,
        string turboFilter
    )
    {
        var clientsPath = Path.GetFullPath("../../Clients", builder.AppHostDirectory);

        return builder.AddDockerfileBuilder(
            name,
            clientsPath,
            context =>
            {
                context
                    .Builder.From("oven/bun:1", BuildStage)
                    .WorkDir("/app")
                    .Copy(".", ".")
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
