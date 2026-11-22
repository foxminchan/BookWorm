using Aspire.Hosting.Yarp;
using BookWorm.AppHost.Extensions.Proxy;

namespace BookWorm.AppHost.Extensions.Frontend;

internal static class FrontendExtensions
{
    private const string MicrofrontendsConfigRelativePath =
        "../../Clients/apps/storefront/microfrontends.json";

    extension(IDistributedApplicationBuilder builder)
    {
        public (
            IResourceBuilder<IResourceWithEndpoints> Storefront,
            IResourceBuilder<IResourceWithEndpoints> Commerce,
            IResourceBuilder<IResourceWithEndpoints> Backoffice
        ) AddFrontendApps(
            IResourceBuilder<YarpResource> gateway,
            IResourceBuilder<EntraIdApplicationResource> entraId
        )
        {
            var workspace = builder
                .AddFrontendWorkspace(
                    Components.TurboRepo,
                    Path.GetFullPath("../../Clients", builder.AppHostDirectory)
                )
                .WithGateway(gateway)
                .WithEntraId(entraId);

            var storefront = workspace
                .AddApp(Clients.StoreFront, Clients.StoreFrontTurboApp)
                .WithCopilot()
                .Build();

            var commerce = workspace
                .AddApp(Clients.Commerce, Clients.CommerceTurboApp)
                .WithHealthCheckPath("/api/health")
                .WithoutEntraId()
                .Build();

            var backoffice = workspace
                .AddApp(Clients.BackOffice, Clients.BackOfficeTurboApp)
                .WithPathPrefix("/backoffice")
                .WithHealthCheckPath("/backoffice")
                .Build();

            return (storefront, commerce, backoffice);
        }

        private FrontendWorkspaceBuilder AddFrontendWorkspace(string name, string workingDirectory)
        {
            return new(builder, name, workingDirectory);
        }
    }

    extension(ApiGatewayProxyBuilder gatewayBuilder)
    {
        public ApiGatewayProxyBuilder WithMicrofrontends(
            IResourceBuilder<IResourceWithEndpoints> storefront,
            IResourceBuilder<IResourceWithEndpoints> commerce
        )
        {
            var microfrontendsConfigPath = Path.GetFullPath(
                MicrofrontendsConfigRelativePath,
                gatewayBuilder.Resource.ApplicationBuilder.AppHostDirectory
            );

            return gatewayBuilder
                .WithMicrofrontends(
                    microfrontendsConfigPath,
                    new Dictionary<string, IResourceBuilder<IResourceWithEndpoints>>
                    {
                        [Clients.Commerce] = commerce,
                    }
                )
                .WithFrontend(storefront, zoneName: Clients.StoreFront);
        }
    }
}
