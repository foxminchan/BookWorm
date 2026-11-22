namespace BookWorm.AppHost.Extensions.Frontend;

internal sealed class FrontendAppBuilder(
    FrontendWorkspaceBuilder workspace,
    string name,
    string turboFilter
)
{
    private const string AppUrlEnvironmentVariable = "NEXT_PUBLIC_APP_URL";
    private const string BuildStage = "build";

    private bool _includeCopilot;
    private string? _pathPrefix;

    public FrontendAppBuilder WithCopilot()
    {
        _includeCopilot = true;
        return this;
    }

    public FrontendAppBuilder WithPathPrefix(string pathPrefix)
    {
        _pathPrefix = pathPrefix;
        return this;
    }

    public IResourceBuilder<IResourceWithEndpoints> Build()
    {
        return workspace.Turbo is not null
            ? Configure(
                workspace
                    .Turbo.AddApp(name, turboFilter)
                    .WithHttpEndpoint(env: "PORT")
                    .WithMappedEndpointPort()
            )
            : Configure(AddContainer().WithHttpEndpoint(targetPort: 3000, env: "PORT"));
    }

    private IResourceBuilder<TResource> Configure<TResource>(IResourceBuilder<TResource> frontend)
        where TResource : IResourceWithEnvironment, IResourceWithEndpoints
    {
        var isRunMode = workspace.Builder.ExecutionContext.IsRunMode;
        var gatewayOrigin = ReferenceExpression.Create(
            $"{workspace.Gateway.GetBrowserEndpoint().PrimaryBrowserOrigin}"
        );

        frontend
            .WithOtlpExporter()
            .WithHttpHealthCheck()
            .WithEnvironment(
                "NEXT_PUBLIC_GATEWAY_HTTPS",
                isRunMode ? workspace.Gateway.GetEndpoint(Uri.UriSchemeHttps) : gatewayOrigin
            )
            .WithEnvironment(
                "NEXT_PUBLIC_GATEWAY_HTTP",
                workspace.Gateway.GetEndpoint(Uri.UriSchemeHttp)
            )
            .WithEnvironment(
                AppUrlEnvironmentVariable,
                string.IsNullOrEmpty(_pathPrefix)
                    ? gatewayOrigin
                    : ReferenceExpression.Create($"{gatewayOrigin}{_pathPrefix}")
            );

        if (_includeCopilot)
        {
            frontend.WithEnvironment("NEXT_PUBLIC_COPILOT_ENABLED", "true");
        }

        return frontend.WithEntraId(workspace.EntraId, workspace.Gateway, _pathPrefix);
    }

    private IResourceBuilder<ContainerResource> AddContainer()
    {
        return workspace.Builder.AddDockerfileBuilder(
            name,
            workspace.WorkingDirectory,
            context =>
            {
                var buildStage = context
                    .Builder.From("oven/bun:1-alpine", BuildStage)
                    .WorkDir("/app")
                    .Copy(".", ".");

                if (_includeCopilot)
                {
                    buildStage.Env("NEXT_PUBLIC_COPILOT_ENABLED", "true");
                }

                buildStage
                    .Run("bun install --frozen-lockfile")
                    .Run($"bunx turbo build --filter={turboFilter}");

                context
                    .Builder.From("oven/bun:1-alpine", "runtime")
                    .WorkDir("/app")
                    .CopyFrom(BuildStage, $"/app/apps/{name}/.next/standalone", ".")
                    .CopyFrom(
                        BuildStage,
                        $"/app/apps/{name}/.next/static",
                        $"./apps/{name}/.next/static"
                    )
                    .CopyFrom(BuildStage, $"/app/apps/{name}/public", $"./apps/{name}/public")
                    .Env("NODE_ENV", "production")
                    .Env("NEXT_TELEMETRY_DISABLED", "1")
                    .Env("HOSTNAME", "0.0.0.0")
                    .Expose(3000)
                    .Entrypoint(["bun", $"apps/{name}/server.js"]);

                return Task.CompletedTask;
            },
            "runtime"
        );
    }
}
