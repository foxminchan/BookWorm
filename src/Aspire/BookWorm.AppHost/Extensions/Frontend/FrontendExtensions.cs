namespace BookWorm.AppHost.Extensions.Frontend;

internal static class FrontendExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        public void AddFrontendApps(
            IResourceBuilder<Aspire.Hosting.Yarp.YarpResource> gateway,
            IResourceBuilder<IResource> keycloak
        )
        {
            if (builder.ExecutionContext.IsRunMode)
            {
                AddTurborepoApps(builder, gateway, keycloak);
                return;
            }

            AddPublishedContainerApps(builder, gateway, keycloak);
        }
    }

    private static void AddTurborepoApps(
        IDistributedApplicationBuilder builder,
        IResourceBuilder<Aspire.Hosting.Yarp.YarpResource> gateway,
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
            "NEXT_PUBLIC_APP_URL",
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
            "NEXT_PUBLIC_APP_URL",
            backoffice.GetEndpoint(Uri.UriSchemeHttp)
        );
    }

    private static void AddPublishedContainerApps(
        IDistributedApplicationBuilder builder,
        IResourceBuilder<Aspire.Hosting.Yarp.YarpResource> gateway,
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
            includeCopilot: true
        );

        storefront.WithEnvironment(
            "NEXT_PUBLIC_APP_URL",
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
            "NEXT_PUBLIC_APP_URL",
            backoffice.GetEndpoint(Uri.UriSchemeHttp)
        );
    }

    private static IResourceBuilder<ContainerResource> ConfigurePublishedFrontend(
        IResourceBuilder<ContainerResource> frontend,
        IResourceBuilder<Aspire.Hosting.Yarp.YarpResource> gateway,
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
                    .Builder.From("oven/bun:1", "build")
                    .WorkDir("/app")
                    .Copy(".", ".")
                    .Run("bun install --frozen-lockfile")
                    .Run($"bunx turbo build --filter={turboFilter}");

                context
                    .Builder.From("oven/bun:1-alpine", "runtime")
                    .WorkDir("/app")
                    .CopyFrom("build", $"/app/apps/{appName}/.next/standalone", ".")
                    .CopyFrom(
                        "build",
                        $"/app/apps/{appName}/.next/static",
                        $"./apps/{appName}/.next/static"
                    )
                    .CopyFrom("build", $"/app/apps/{appName}/public", $"./apps/{appName}/public")
                    .Env("NODE_ENV", "production")
                    .Env("NEXT_TELEMETRY_DISABLED", "1")
                    .Env("HOSTNAME", "0.0.0.0")
                    .Expose(3000)
                    .Entrypoint(["bun", $"apps/{appName}/server.js"]);

                return Task.CompletedTask;
            },
            stage: "runtime"
        );
    }
}
