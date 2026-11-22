using Aspire.Hosting.Yarp;

namespace BookWorm.AppHost.Extensions.Frontend;

internal sealed class FrontendWorkspaceBuilder
{
    private IResourceBuilder<EntraIdApplicationResource>? _entraId;
    private IResourceBuilder<YarpResource>? _gateway;

    internal FrontendWorkspaceBuilder(
        IDistributedApplicationBuilder builder,
        string name,
        string workingDirectory
    )
    {
        Builder = builder;
        WorkingDirectory = workingDirectory;
        Turbo = builder.ExecutionContext.IsRunMode
            ? builder
                .AddTurborepoApp(name, workingDirectory)
                .WithBun(true)
                .WithPackageManagerLaunch()
            : null;
    }

    internal IDistributedApplicationBuilder Builder { get; }

    internal string WorkingDirectory { get; }

    internal IResourceBuilder<TurborepoResource>? Turbo { get; }

    internal IResourceBuilder<YarpResource> Gateway =>
        _gateway ?? throw new InvalidOperationException("Gateway must be configured.");

    internal IResourceBuilder<EntraIdApplicationResource> EntraId =>
        _entraId ?? throw new InvalidOperationException("Entra ID must be configured.");

    public FrontendWorkspaceBuilder WithGateway(IResourceBuilder<YarpResource> gateway)
    {
        _gateway = gateway;
        return this;
    }

    public FrontendWorkspaceBuilder WithEntraId(
        IResourceBuilder<EntraIdApplicationResource> entraId
    )
    {
        _entraId = entraId;
        return this;
    }

    public FrontendAppBuilder AddApp(string name, string turboFilter)
    {
        return new(this, name, turboFilter);
    }
}
